namespace CdnGetter;

public partial class ParsedUri
{
    public class PathSegment : IEquatable<PathSegment>, IComparable<PathSegment>
    {
        public char? Separator { get; }

        public string Name { get; }
        
        public PathSegment(char separator, string name) => (Separator, Name) = (separator, name ?? "");

        public static readonly PathSegment EmptyRoot = new(PATH_DELIMITER_CHAR, string.Empty);
        
        public PathSegment(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty if no separator is provided.", nameof(name));
            Name = name;
        }

        public bool Equals(PathSegment? other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj) => Equals(obj as PathSegment);

        public int CompareTo(PathSegment? other)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
