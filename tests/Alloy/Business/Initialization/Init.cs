[assembly: System.Web.PreApplicationStartMethod(typeof(Example.Init), nameof(Example.Init.Start))]

namespace Example
{
    public class Init
    {
        /// <summary>
        /// Run pre start code
        /// </summary>
        public static void Start()
        {
        //    AbstractEpiserverIoc.Abstractions.EpiserverEnvironment.EnvironmentNameProvider =
        //        () => System.Web.Configuration.WebConfigurationManager.AppSettings["episerver:environment"];
        }
    }
}
