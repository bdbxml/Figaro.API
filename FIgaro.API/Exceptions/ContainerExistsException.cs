using System;

namespace Figaro.API.Exceptions
{
    public class ContainerExistsException: ApplicationException
    {
        public ContainerExistsException(): base("Container exists in collection.") { }
        
    }
}