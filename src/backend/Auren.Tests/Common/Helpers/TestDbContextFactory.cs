using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auren.Tests.Common.Helpers
{
    public static class TestDbContextFactory
    {
        public static AurenDbContext CreateAppDb()
        {
            var options = new DbContextOptionsBuilder<AurenDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AurenDbContext(options);
        }

        public static AurenAuthDbContext CreateAuthDb()
        {
            var options = new DbContextOptionsBuilder<AurenAuthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AurenAuthDbContext(options);
        }
    }
}
