using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using NobatPlusAPI.Models.FileCenter;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.Security.Claims;

[Route("FileCenter")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class FileCenterController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogRep _logRep;
    private readonly IFileUploadRep _fileUploadRep;
    private readonly IImageRep _imageRep;

    public FileCenterController(IWebHostEnvironment env, ILogRep logRep, IFileUploadRep fileUploadRep, IImageRep imageRep)
    {
        _env = env;
        _logRep = logRep;
        _fileUploadRep = fileUploadRep;
        _imageRep = imageRep;
    }

    [HttpPost("uploadfile")]
    [RequestSizeLimit(2_000_000_000)]           // 2 GB
    [RequestFormLimits(MultipartBodyLengthLimit = 2_000_000_000)]
    [AllowAnonymous]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] bool isPublic, [FromQuery] string entityName, [FromQuery] string fileType, [FromQuery] long rowId = 0)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایلی انتخاب نشده است.");
            fileType = fileType.ToLower();
            entityName = entityName.ToLower();
            string fileName = file.FileName.GenerateFileName();
            string fullPath = ""; long RowNumber = 0;
            var userId =  User?.FindFirst("userId")?.Value;
            if ((string.IsNullOrEmpty(userId) || userId == "0")) return Unauthorized();
            string savePath = isPublic
                ? Path.Combine(_env.ContentRootPath, "FileCenter", entityName, fileType, "Public")
                : Path.Combine(_env.ContentRootPath, "FileCenter", entityName, fileType, userId);
            Directory.CreateDirectory(savePath);
            long resultId = 0;
            string downloadUrl = "";
            if (fileType == "images")
            {
                RowNumber = await _imageRep.GetNewRowNumber();
                //fileName = $"{entityName}_{RowNumber}_{userId}{Path.GetExtension(file.FileName)}";
                fullPath = Path.Combine(savePath, fileName);
                Image theImage = new()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Description = isPublic ? "Public" : "Private",
                    FileName = fileName,
                    FilePath = fullPath,
                    EntityType = entityName,
                    ForeignKeyId = rowId <= 0 ? RowNumber : rowId,
                    FileNumber = RowNumber,
                    CreatorId = long.Parse(userId),
                    
                };

                var removeoldResult = await _imageRep.RemoveOldImagesAsync(theImage.ForeignKeyId, entityName);
                if (!removeoldResult.Status) return BadRequest(removeoldResult);

                var saveResult = await _imageRep.AddImagesAsync(new List<Image> { theImage });
                if (!saveResult.Status) return BadRequest(saveResult);
                resultId = saveResult.ID;
                downloadUrl = $"/filecenter/downloadfile?fileType={fileType}&rowId={resultId}&entityName={entityName}";
                theImage.GetUrl = downloadUrl;
                await _imageRep.EditImagesAsync(new List<Image>() { theImage });
            }
            else if (fileType == "files")
            {
                RowNumber = await _fileUploadRep.GetNewRowNumber();
                //fileName = $"{entityName}_{RowNumber}_{userId}{Path.GetExtension(file.FileName)}";
                fullPath = Path.Combine(savePath, fileName);
                FileUpload theFile = new()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    FileName = fileName,
                    FilePath = fullPath,
                    EntityType = entityName,
                    ForeignKeyId = rowId <= 0 ? RowNumber : rowId,
                    ContentType = fullPath.GetContentType(),
                    Description = isPublic ? "Public" : "Private",
                    CreatorId = long.Parse(userId),
                };

                var removeoldResult = await _fileUploadRep.RemoveOldFilesAsync(theFile.ForeignKeyId, entityName);
                if (!removeoldResult.Status) return BadRequest(removeoldResult);

                var saveResult = await _fileUploadRep.AddFileUploadAsync(theFile);
                if (!saveResult.Status) return BadRequest(saveResult);
                resultId = saveResult.ID;
                downloadUrl = $"/filecenter/downloadfile?fileType={fileType}&rowId={resultId}&entityName={entityName}";
                theFile.GetUrl = downloadUrl;
                await _fileUploadRep.EditFileUploadAsync(theFile);
            }
            else return BadRequest("Invalid File Category!");
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            Log log = new()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = $"UploadFile:{{Entity={entityName},Type={fileType},Row={rowId},Path={fullPath},Id={resultId}}}",
            };
            await _logRep.AddLogAsync(log);
            return Ok(new
            {
                success = true,
                fileName,
                resultId,
                url = downloadUrl,
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"{ex.Message} - {ex.InnerException?.Message}");
        }
    }

    [HttpGet("downloadfile")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadFile([FromQuery] string fileType, [FromQuery] long rowId = 0, [FromQuery] long foreignkeyId = 0, [FromQuery] string entityName = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileType))
                return BadRequest("نوع فایل مشخص نشده است.");

            fileType = fileType.Trim().ToLower();
            entityName = entityName?.Trim().ToLower() ?? string.Empty;

            string filePath = string.Empty;
            long userId = 0;
            long roleId = 0;

            if (User.Identity?.IsAuthenticated == true)
            {
                long.TryParse(User.FindFirst("userId")?.Value, out userId);
                long.TryParse(User.FindFirst("Role")?.Value, out roleId);
            }

            if (fileType.ToLower() == "images")
            {
                var theImage = await _imageRep.GetImageForShowAsync(rowId, foreignkeyId, entityName, userId, roleId);
                if (theImage == null || !theImage.Status || theImage.Result == null)
                {
                    return BadRequest(theImage?.ErrorMessage ?? "تصویر یافت نشد.");
                }

                filePath = theImage.Result.FilePath;
            }
            else if (fileType.ToLower() == "files")
            {
                var theFile = await _fileUploadRep.GetFileForDownloadAsync(rowId, foreignkeyId, entityName, userId, roleId);
                if (theFile == null || !theFile.Status || theFile.Result == null)
                {
                    return BadRequest(theFile?.ErrorMessage ?? "فایل یافت نشد.");
                }

                filePath = theFile.Result.FilePath;
            }
            else
            {
                return BadRequest("نوع فایل نامعتبر است.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest("مسیر فایل ثبت نشده است.");
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("فایل روی سرور پیدا نشد.");
            }

            var contentType = filePath.GetContentType();
            var fileName = Path.GetFileName(filePath);

            Log log = new()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = $"DownloadFile:{{Entity={entityName},Type={fileType},Row={rowId},FK={foreignkeyId},Path={filePath}}}",
            };
            await _logRep.AddLogAsync(log);

            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                useAsync: true);

            return File(stream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            return BadRequest($"{ex.Message} - {ex.InnerException?.Message}");
        }
    }

    [HttpPost("GetDownloadLinks_Base")]
    public async Task<ActionResult<ListResultObject<string>>> GetDownloadLinks_Base(GetFileCenterDownloadListRequestBody requestBody)
    {
        requestBody.entityType = requestBody.entityType.ToLower();
        requestBody.fileType = requestBody.fileType.ToLower();

        var result = new ListResultObject<string>();
        dynamic resultrecords;

        if (!ModelState.IsValid)
        {
            return BadRequest(requestBody);
        }
        switch (requestBody.fileType)
        {
            default:
            case "files":
                {
                    resultrecords = await _fileUploadRep.GetAllFileUploadsAsync(requestBody.entityType, requestBody.ForeignKeyId, 0, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
                }
                break;
            case "images":
                {
                    resultrecords = await _imageRep.GetAllImagesAsync(requestBody.entityType, requestBody.ForeignKeyId, 0, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
                }
                break;
        }

        result.ErrorMessage = resultrecords.ErrorMessage;
        result.Status = resultrecords.Status;
        result.PageCount = resultrecords.PageCount;
        result.TotalCount = resultrecords.TotalCount;

        if (requestBody.fileType == "images")
        {
            result.Results = ((List<Image>)resultrecords.Results)
  .Select(x => $"{Request.Scheme}://{Request.Host}{x.GetUrl}").ToList();
        }
        if (requestBody.fileType == "files")
        {
            result.Results = ((List<FileUpload>)resultrecords.Results)
  .Select(x => $"{Request.Scheme}://{Request.Host}{x.GetUrl}").ToList();
        }

        if (result.Status)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }



}