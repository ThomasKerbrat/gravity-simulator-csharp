
namespace gravity_simulator_csharp
{
	public interface ISeedStrategy
	{
		Body NextBody(IUniverse universe);
	}
}
