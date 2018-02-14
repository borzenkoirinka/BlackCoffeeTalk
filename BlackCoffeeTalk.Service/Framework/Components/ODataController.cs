using System;

namespace BlackCoffeeTalk.Framework
{
    public abstract class ODataController<TEntity> : ControllerBase where TEntity : class, new()
    {
        public override Type ModelType => typeof(TEntity);
    }
}