//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Rhino.Controllers.Models.Server;

//namespace Rhino.Controllers.Controllers
//{
//    [ApiVersion("3.0")]
//    [Route("api/v{version:apiVersion}/[controller]")]
//    [ApiController]
//    public class ResourcesController : ControllerBase
//    {
//        //[HttpPost("gravity/plugins")]
//        public async Task<IActionResult> AddPlugins(IFormFile file)
//        {
//            //// open file
//            //using var readStream = new StreamReader(file.OpenReadStream());
            
//            //// read
//            //var content = await readStream.ReadToEndAsync();

//            //// write
//            //System.IO.File.WriteAllText(file.Name, content);
            
//            return Ok();
//        }

//        public IActionResult GetResources()
//        {
//            return Ok();
//        }

//        [HttpPost("resources")]
//        public IActionResult CreateResource([FromBody] ResourceFileModel model)
//        {
//            // assert
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            // invoke
//            var status = CreateResourceFile(model);

//            // get
//            return new StatusCodeResult(status);
//        }

//        [HttpPost("resources/bulk")]
//        public IActionResult CreateResources([FromBody] IEnumerable<ResourceFileModel> model)
//        {
//            return Ok();
//        }

//        private static int CreateResourceFile(ResourceFileModel model)
//        {
//            try
//            {
//                // normalize
//                model.FileName = !model.FileName.ToUpper().EndsWith(".TXT")
//                    ? $"{model.FileName}.txt"
//                    : model.FileName;

//                // setup
//                var path = Path.Combine(Environment.CurrentDirectory, "Resources", model.Path, model.FileName);

//                // write
//                System.IO.File.WriteAllText(path, model.Content);

//                // get
//                return StatusCodes.Status204NoContent;
//            }
//            catch (Exception e) when (e != null)
//            {
//                return StatusCodes.Status423Locked;
//            }
//        }
//    }
//}
