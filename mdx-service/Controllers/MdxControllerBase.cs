using MdxServices.Interfaces;
using MdxServices.Models;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AspNetCore.Mvc;

namespace MdxServices.Controllers
{
    [ApiController]
    public abstract class MdxControllerBase : ControllerBase
    {
        protected readonly IMdxService _mdxService;
        protected readonly ILogger _logger;

        protected MdxControllerBase(IMdxService mdxService, ILogger logger)
        {
            _mdxService = mdxService;
            _logger = logger;
        }

        protected IActionResult RunQueries <T>(string mdxQuery, Func<IEnumerable<Dictionary<string, object?>>, IEnumerable<T>> mapper)
        {
            try
            {
                var data = _mdxService.Execute(mdxQuery).ToList();
                var mapped = mapper(data).ToList();
                return Ok(ApiResponse<IEnumerable<T>>.Ok(mapped, mapped.Count));
            }
            catch (AdomdErrorResponseException ex)
            {
                _logger.LogError(ex, "ADOMD error response");
                return StatusCode(422, ApiResponse<object>.Fail($"ADOMD error: {ex.Message}"));
            }
            catch (AdomdConnectionException ex)
            {
                _logger.LogError(ex, "ADOMD connection failed");
                return StatusCode(503, ApiResponse<object>.Fail($"Connection error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing MDX");
                return StatusCode(500, ApiResponse<object>.Fail($"Internal error: {ex.Message}"));
            }
        }
        protected IActionResult RunQuery(string mdxQuery) {
            try {
                var data = _mdxService.Execute(mdxQuery).ToList();
                return Ok(ApiResponse<IEnumerable<Dictionary<string, object?>>>.Ok(data, data.Count)); 
            } catch (AdomdErrorResponseException ex) { 
                _logger.LogError(ex, "ADOMD error response");
                return StatusCode(422, ApiResponse<object>.Fail($"ADOMD error: {ex.Message}")); 
            } catch (AdomdConnectionException ex) {
                _logger.LogError(ex, "ADOMD connection failed");
                return StatusCode(503, ApiResponse<object>.Fail($"Connection error: {ex.Message}")); 
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error executing MDX");
                return StatusCode(500, ApiResponse<object>.Fail($"Internal error: {ex.Message}")); 
            } 
        }
    }
}