using Journals.Domain.Journals.Enums;
using Redis.OM.Modeling;

namespace Journals.Domain.Journals.ValueObjects;

[Document]
public class SectionEditor
{
    //public int SectionId { get; set; }
    [Indexed] 
    public int EditorId { get; set; }
        
    public EditorRole EditorRole { get; set; }
}
