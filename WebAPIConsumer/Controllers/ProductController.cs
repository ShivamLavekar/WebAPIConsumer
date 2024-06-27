using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using WebAPIConsumer.Models;

namespace WebAPIConsumer.Controllers
{
    public class ProductController : Controller
    {
        HttpClient client;
        public ProductController()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            client = new HttpClient(clientHandler);
        }

        public IActionResult Index()
        {
            List<Product> products = new List<Product>();
            string url = "https://localhost:44339/api/Product/GetProd";
            HttpResponseMessage response=client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata=response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<List<Product>>(jsondata);
                if (obj != null)
                {
                    products=obj;
                }
            }
            return View(products);
        }

        public IActionResult AddProduct() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product p, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var imagePath = UploadFile(imageFile).Result;
                p.pimage = imagePath;  // Assuming your property is named "Pimage" in the Product model
            }

            string url = "https://localhost:44339/api/Product/AddProd/";
            var jsondata = JsonConvert.SerializeObject(p);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult DeleteProduct(int id)
        {
            string url = $"https://localhost:44339/api/Product/DelProd/{id}";
            HttpResponseMessage response = client.DeleteAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View();
        }



        [HttpPost]
        public IActionResult DeleteSelectedProducts(List<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Count > 0)
            {
                foreach (var id in selectedIds)
                {
                    string url = $"https://localhost:44339/api/Product/DelProd/{id}";
                    HttpResponseMessage response = client.DeleteAsync(url).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        // Handle error
                        ViewBag.ErrorMessage = "Error deleting some products.";
                        break;
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            string url = $"https://localhost:44339/api/Product/GetProductById/{id}";
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsondata = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<Product>(jsondata);
                if (obj != null)
                {
                    return View(obj);
                }
            }
            return View();
        }


        [HttpPost]
        public IActionResult EditProduct(Product p, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var imagePath = UploadFile(imageFile).Result;
                p.pimage = imagePath;  // Assuming your property is named "Pimage" in the Product model
            }

            string url = $"https://localhost:44339/api/Product/UpdateProd/{p.pid}";
            var jsondata = JsonConvert.SerializeObject(p);
            StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(p);
        }

        private async Task<string> UploadFile(IFormFile file)
{
    if (file != null && file.Length > 0)
    {
        var url = "https://localhost:44339/api/Product/upload";
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StreamContent(file.OpenReadStream())
            {
                Headers =
                {
                    ContentLength = file.Length,
                    ContentType = new MediaTypeHeaderValue(file.ContentType)
                }
            }, "file", file.FileName);

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var filePath = JsonConvert.DeserializeObject<dynamic>(responseData).filePath;
                return filePath;
            }
        }
    }
    return null;
}

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var url = $"https://localhost:44339/api/Product/download/{fileName}/";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return NotFound("File not found.");

            var fileStream = await response.Content.ReadAsStreamAsync();
            var contentType = response.Content.Headers.ContentType.ToString();

            return File(fileStream, contentType, fileName);
        }




    }
}
