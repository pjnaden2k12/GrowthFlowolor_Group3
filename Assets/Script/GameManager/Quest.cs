[System.Serializable]
public class Quest
{
    public string title;
    public int target;
    public int currentProgress;
    public bool isCompleted;
    public bool isClaimed;

    public bool CheckCompleted()
    {
        if (!isCompleted && currentProgress >= target)
        {
            isCompleted = true;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        currentProgress = 0;
        isCompleted = false;
        isClaimed = false;
    }
}
