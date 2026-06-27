namespace Auren.Domain.Common
{
	public interface IEntity
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
	}
}
