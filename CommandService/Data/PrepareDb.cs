using CommandService.Models;
using CommandService.SyncDataServices.Grpc;

namespace CommandService.Data
{
    public class PrepareDb
    {
        public static void Populate(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var grpcClient = scope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcClient.GetAllPlatforms();

                Seed(scope.ServiceProvider.GetService<ICommandRepository>(), platforms);
            }
        }

        private static void Seed(ICommandRepository repository, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding new platforms...");

            foreach (var platform in platforms)
            {
                if (!repository.ExternalPlatformExists(platform.ExternalId))
                {
                    repository.CreatePlatform(platform);
                }
                repository.SaveChanges();
            }
        }
    }
}
