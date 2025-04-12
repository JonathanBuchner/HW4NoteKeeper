using System.Net.Mail;

namespace HW4NoteKeeperEx2.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to handle requests delete and patch requests missing id
    /// </summary>
    public class MethodNotAllowedMiddleware
    {
        /// <summary>
        /// Next middleware in the pipeline
        /// </summary>
        private readonly RequestDelegate _next;

        public MethodNotAllowedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to handle requests that are not allowed
        /// </summary>
        /// <param name="context">context for request</param>
        public async Task Invoke(HttpContext context)
        {
            if (PatchOrDeleteRequestsIsMissingId(context))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                
                await context.Response.WriteAsync("Must include Id or method not allowed.");
                
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Checks if PATCH or DELETE requests are missing an id
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool PatchOrDeleteRequestsIsMissingId(HttpContext context)
        {
            var request = context.Request;

            // Check if method is PATCH or DELETE
            if (request.Method == HttpMethods.Patch || request.Method == HttpMethods.Delete)
            {
                var routeData = context.GetRouteData();

                if (routeData == null || routeData.Values == null)
                {
                    return false;
                }

                if (routeData.Values.ContainsKey("controller"))
                {
                    if (routeData.Values["controller"] == null)
                    {
                        return false;
                    }

                    // Notes controller
                    if (routeData.Values["controller"].ToString().ToLower() == "notes")
                    {
                        if (!routeData?.Values.ContainsKey("id") ?? true)
                        {
                            return true;
                        }
                    }

                    // Attachment controller
                    if (routeData.Values["controller"].ToString().ToLower() == "attachments")
                    {
                        if (!routeData?.Values.ContainsKey("noteId") ?? true)
                        {
                            return true;
                        }
                    }

                    // AttachmentZip controller
                    if (routeData.Values["controller"].ToString().ToLower() == "attachmentzip")
                    {
                        if (!routeData?.Values.ContainsKey("noteId") ?? true)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
