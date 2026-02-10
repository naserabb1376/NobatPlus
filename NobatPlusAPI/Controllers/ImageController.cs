using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Image;
using NobatPlusAPI.Models.Public;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using Domains;
using FileIO = System.IO.File;

namespace NobatPlusAPI.Controllers
{
    [Route("Image")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class ImageController : ControllerBase
    {
        IImageRep _ImageRep;
        ILogRep _logRep;

        public ImageController(IImageRep ImageRep,ILogRep logRep)
        {
           _ImageRep = ImageRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllImages_Base")]
        public async Task<ActionResult<ListResultObject<Image>>> GetAllImages_Base(GetImageListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ImageRep.GetAllImagesAsync(requestBody.EntityType,requestBody.ForeignKeyId,requestBody.CreatorId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetImageById_Base")]
        public async Task<ActionResult<RowResultObject<Image>>> GetImageById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ImageRep.GetImageByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistImage_Base")]
        public async Task<ActionResult<BitResultObject>> ExistImage_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ImageRep.ExistImageAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddImages_Base")]
        public async Task<ActionResult<BitResultObject>> AddImages_Base(List<AddEditImageRequestBody> requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var Images = requestBody.Select(x=> new Image()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Description = x.Description ?? "",
                CreatorId = x.CreatorId ?? 0,
                EntityType = x.EntityType,
                FileName = x.FileName,
                FilePath = x.FilePath,
                ForeignKeyId = x.ForeignKeyId,
                GetUrl = x.GetUrl ?? "",
            }).ToList();
            
            var result = await _ImageRep.AddImagesAsync(Images);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion


                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditImages_Base")]
        public async Task<ActionResult<BitResultObject>> EditImages_Base(List<AddEditImageRequestBody> requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = new BitResultObject();
            var Images = new List<Image>();

            foreach (var body in requestBody)
            {
                var theRow = await _ImageRep.GetImageByIdAsync(body.ID);
                if (!theRow.Status)
                {
                    result.Status = theRow.Status;
                    result.ErrorMessage = theRow.ErrorMessage;
                    return BadRequest(result);
                }

                var Image = new Image
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = body.ID,
                    EntityType = body.EntityType,
                    FileName = body.FileName,
                    FilePath = body.FilePath,
                    ForeignKeyId = body.ForeignKeyId,
                    Description = body.Description ??"",
                    CreatorId = body.CreatorId ?? 0,
                    GetUrl = body.GetUrl ?? "",
                };

                Images.Add(Image);
            }

            result = await _ImageRep.EditImagesAsync(Images);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteImages_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteImages_Base(List<long> ids)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ids);
            }
            foreach (long id in ids)
            {
                var theRow = await _ImageRep.GetImageByIdAsync(id);
                if (theRow.Result != null && FileIO.Exists(theRow.Result.FilePath))
                {
                    FileIO.Delete(theRow.Result.FilePath);
                }
            }
            var result = await _ImageRep.RemoveImagesAsync(ids);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
