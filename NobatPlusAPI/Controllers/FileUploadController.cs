using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.FileUpload;
using NobatPlusAPI.Models.Public;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domains;

namespace NobatPlusAPI.Controllers
{
    [Route("FileUpload")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class FileUploadController : ControllerBase
    {
        private IFileUploadRep _FileUploadRep;
        private ILogRep _logRep;

        public FileUploadController(IFileUploadRep FileUploadRep, ILogRep logRep)
        {
            _FileUploadRep = FileUploadRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllFileUploads_Base")]
        public async Task<ActionResult<ListResultObject<FileUpload>>> GetAllFileUploads_Base(GetFileUploadListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _FileUploadRep.GetAllFileUploadsAsync(requestBody.entityType, requestBody.ForeignKeyId, requestBody.CreatorId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetFileUploadById_Base")]
        public async Task<ActionResult<RowResultObject<FileUpload>>> GetFileUploadById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _FileUploadRep.GetFileUploadByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistFileUpload_Base")]
        public async Task<ActionResult<BitResultObject>> ExistFileUpload_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _FileUploadRep.ExistFileUploadAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddFileUpload_Base")]
        public async Task<ActionResult<BitResultObject>> AddFileUpload_Base(AddEditFileUploadRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            FileUpload FileUpload = new FileUpload()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                ForeignKeyId = requestBody.ForeignKeyId,
                EntityType = requestBody.EntityType,
                ContentType = requestBody.ContentType,
                FileName = requestBody.FileName,
                FilePath = requestBody.FilePath,
                CreatorId = requestBody.CreatorId ?? 0,
                Description = requestBody.Description ?? "",
            };
            var result = await _FileUploadRep.AddFileUploadAsync(FileUpload);
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

                #endregion AddLog

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditFileUpload_Base")]
        public async Task<ActionResult<BitResultObject>> EditFileUpload_Base(AddEditFileUploadRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _FileUploadRep.GetFileUploadByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            FileUpload FileUpload = new FileUpload()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                ForeignKeyId = requestBody.ForeignKeyId,
                EntityType = requestBody.EntityType,
                ContentType = requestBody.ContentType,
                FileName = requestBody.FileName,
                FilePath = requestBody.FilePath,
                CreatorId = requestBody.CreatorId ?? 0,
                Description = requestBody.Description ?? "",
            };
            result = await _FileUploadRep.EditFileUploadAsync(FileUpload);
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

                #endregion AddLog

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("DeleteFileUpload_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteFileUpload_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _FileUploadRep.RemoveFileUploadAsync(requestBody.ID);
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

                #endregion AddLog

                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}