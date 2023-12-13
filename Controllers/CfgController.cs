using Microsoft.AspNetCore.Mvc;
using VisualCfg.BigBrain;

namespace VisualCfg.Controllers
{
    [ApiController]
    [Route("api/cfg")]
    public class CfgController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Visualize(string code)
        {
            var either = BigBrain.Compiler.CompileCfg(code);

            if (either.HasResult)
            {
                return Ok(either.Result);
            }
            else
            {
                return BadRequest(either.Error.ToError());
            }
        }
    }
}
