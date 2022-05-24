using System;

namespace Lomont.Projects
{
    /// <summary>
    /// Tag project classes with this to get auto recognized
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    class ProjectAttribute : Attribute
    {
        public ProjectAttribute(string author, string description, string date)
        {
            Author = author;
            Description = description;
            Date = DateTime.Parse(date);
        }
        public string Author { get; set;  }
        public string Description { get; set;  }
        public DateTime Date { get; set;  }
    }
}
