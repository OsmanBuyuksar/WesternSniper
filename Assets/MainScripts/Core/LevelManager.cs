using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField]
    private GameObject[] levels;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("LEVEL") == false)
        {
            PlayerPrefs.SetInt("LEVEL", 0);
        }

        LoadLevelWithIndex(PlayerPrefs.GetInt("LEVEL"));
    }

    public int GetCurrentLevelIndex()
    {
        return PlayerPrefs.GetInt("LEVELTEXT", 1);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        PlayerPrefs.SetInt("LEVEL", PlayerPrefs.GetInt("LEVEL") + 1);

        PlayerPrefs.SetInt("LEVELTEXT", PlayerPrefs.GetInt("LEVELTEXT", 1) + 1);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadLevelWithIndex(int index)
    {
        if (levels.Length > index)
        {
            for (var i = 0; i < levels.Length; i++)
            {
                levels[i].SetActive(i == index);
            }
        }

        else
        {
            foreach (var level in levels)
                level.SetActive(false);

            var rand = Random.Range(0, levels.Length);

            PlayerPrefs.SetInt("LEVEL", rand);

            levels[rand].SetActive(true);
        }
    }
}