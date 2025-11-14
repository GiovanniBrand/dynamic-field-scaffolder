using Dynamic.Scaffolder.Enums;

namespace MPS.FolhaMais.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GetTypeAttribute : Attribute
    {

        public ComponentType Value { get; }

        public GetTypeAttribute(ComponentType value)
        {
            Value = value;
        }
    }
}
