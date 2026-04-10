public class GUIHelper
{
	private static int m_WinIDCounter = 1340;
	public static int GetFreeWindowID()
	{
		return m_WinIDCounter++;
	}
}	
