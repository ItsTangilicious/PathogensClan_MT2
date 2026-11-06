namespace PathogensClan_MT2.Plugin
{
    [Serializable]
    public class ToxicitySaveData
    {
        public int Toxicity { get; set; }
        public bool IsFullToxicity { get; set; } = false;
    }
}