using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private GameObject[] winScreens;
    [SerializeField] private GameObject[] agentObjects;
    [SerializeField] private Material[] headBandMaterials;
    [SerializeField] private Material[] agentMaterials;
    [SerializeField] private Ease easeType;
    private int winner;

    public void Restart()
    {
        PlayerPrefs.SetInt("Winner", 2);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    private void Start()
    {
        winner = PlayerPrefs.GetInt("Winner", 2);
        winScreens[winner].SetActive(true);
        winScreens[winner].transform.DOScale(1, 1f).SetEase(Ease.OutBack).SetDelay(1f).OnComplete(() =>
        {
            winScreens[winner].transform.GetComponent<Image>().DOFade(0, 1f).SetEase(Ease.InOutSine).SetDelay(3f);
        });

        agentObjects[0].transform.DOLocalMoveY(3f, .5f).SetEase(easeType).SetLoops(-1, LoopType.Yoyo);
        agentObjects[1].transform.DOLocalMoveY(3f, .5f).SetEase(easeType).SetLoops(-1, LoopType.Yoyo).SetDelay(0.5f);


        if (winner == 2)
        {
            for (int i = 0; i < agentObjects.Length; i++)
            {
                agentObjects[i].GetComponent<Renderer>().material = agentMaterials[i];
                agentObjects[i].transform.GetChild(0).GetComponent<Renderer>().material = headBandMaterials[i];
            }
            return;


        }
        foreach (var agent in agentObjects)
        {
            agent.GetComponent<Renderer>().material = agentMaterials[winner];
            agent.transform.GetChild(0).GetComponent<Renderer>().material = headBandMaterials[winner];
        }







    }
}
