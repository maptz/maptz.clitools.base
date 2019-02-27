using System.Threading.Tasks;
namespace Maptz.CliTools
{

    public interface ICliProgramRunner
    {
        /* #region Public Methods */
        Task RunAsync(string[] args);
        /* #endregion Public Methods */
    }
}