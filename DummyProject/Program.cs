namespace DummyProject {
    // This project only exists to make the github Actions node appear in the Solution Explorer !!!!
    public class Program {
       
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello my dummy World!");

            app.Run();
        }
    }
}
