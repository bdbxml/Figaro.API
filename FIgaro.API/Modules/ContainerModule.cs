using System;
using Nancy;
using Nancy.ModelBinding;
using Figaro.API.Exceptions;
using Figaro.API.Models;

namespace Figaro.API
{
    public sealed class ContainerModule: NancyModule
    {
        private FigaroDataContext fdm;
        public ContainerModule(FigaroDataContext manager)
        {
            fdm = manager;

            
            //Create container(s) based on uploaded config inputs
            Post("/containers", args =>
            {
                using (var tx = fdm.Manager.CreateTransaction())
                {

                    try
                    {
                        var model = this.Bind<ContainerElementCollectionType>();
                        var code = fdm.PostContainersToAccount(model, tx);
                        tx.Commit();
                        return code;
                    }
                    catch (Exception e)
                    {
                        tx.Abort();
                        Response.Context.Response.StatusCode = HttpStatusCode.InternalServerError;
                        Response.Context.Response.ReasonPhrase = $"{e.GetType()}: {e.Message}. Transaction aborted.";
                        return Response;
                    }
                }

            });

            Delete("/containers", args =>
            {
                using (var tx = fdm.Manager.CreateTransaction())
                {
                    try
                    {
                        fdm.DeleteContainersFromAccount(tx);
                        tx.Commit();
                    }
                    catch (Exception)
                    {
                        tx.Abort();
                    }
                    return HttpStatusCode.OK;
                }
            });

            Get("/containers", args =>
            {
                return HttpStatusCode.OK;
                //fdm.Manager.
            });


        }
    }
}
