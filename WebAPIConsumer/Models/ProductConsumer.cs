namespace WebAPIConsumer.Models
{
    public class ProductConsumer
    {
       


            public int pid { get; set; }
            public string pimage { get; set; }
            public string pname { get; set; }

            public string pcat { get; set; }
            public int price { get; set; }

            public IFormFile formFile { get; set; }
       
    }
}
