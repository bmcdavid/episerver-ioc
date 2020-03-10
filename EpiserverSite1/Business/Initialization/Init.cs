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
            //throw new System.Exception("here");            
        }
    }
}
