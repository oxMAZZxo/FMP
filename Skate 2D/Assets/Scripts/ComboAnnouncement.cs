
public class ComboAnnouncement
{
    public short comboCount {get;}
    public string[] taglines {get;}

    public ComboAnnouncement(short newComboCount, params string[] newTaglines)
    {
        comboCount = newComboCount;
        taglines = newTaglines;
    }


}