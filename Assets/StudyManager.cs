using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Depthkit;
using UnityEngine.Video;
using BuildingVolumes.Streaming;
using static UnityEditor.Experimental.GraphView.GraphView;
using JetBrains.Annotations;

public class StudyManager : MonoBehaviour
{
    enum compareVariants
    {
        DepthkitToLivescan,
        LiveScanToVolcap,
        VolcapToDepthkit
    }

    enum Candidates
    {
        Depthkit,
        LiveScan,
        VolCap
    }

    public StudyComparisions comparisionList;
    List<compareVariants> currentComparisionVariants;

    public Transform podest1;
    public Transform podest2;

    public GameObject depthkitPlayer;
    public GameObject liveScanPlayer;
    public GameObject volcapPlayer;

    GameObject currentPlayer1;
    GameObject currentPlayer2;
    Candidates currentLeftCandidate;
    Candidates currentRightCandidate;
    compareVariants currentVariant;

    public GameObject voteButton1;
    public GameObject voteButton2;
    public GameObject RepeatButton;

    public GameObject calibrationHint;
    public GameObject StudyStartHint1;
    public GameObject StudyStartHint2;
    public GameObject StudyStartHint3;
    public GameObject PostTrainingHint;
    public GameObject EndStudyHint;
    public GameObject training;
    public GameObject Countdown;

    public string trainingVideoGood;
    public string trainingVideoBad;

    VideoCollection currentVideoCollection;

    int currentComparisionIndex = 0;
    int currentComparisionVariantIndex = 0;
    bool inTraining;
    int views = 0;

    // Start is called before the first frame update
    private void Start()
    {
        ResetStudy();
    }

    public void ResetStudy()
    {
        calibrationHint.SetActive(true);
        StudyStartHint1.SetActive(false);
        StudyStartHint2.SetActive(false);
        StudyStartHint3.SetActive(false);
        PostTrainingHint.SetActive(false);
        EndStudyHint.SetActive(false);

        voteButton1.SetActive(false);
        voteButton2.SetActive(false);
        RepeatButton.SetActive(false);

        currentComparisionIndex = 0;
        currentComparisionVariantIndex = 0;
    }

    public void StartTraining()
    {
        inTraining = true;

        liveScanPlayer.GetComponent<GeometrySequencePlayer>().LoadSequence(trainingVideoBad, GeometrySequenceStream.PathType.AbsolutePath);
        volcapPlayer.GetComponent<GeometrySequencePlayer>().LoadSequence(trainingVideoGood, GeometrySequenceStream.PathType.AbsolutePath);

        PreparePlayer(liveScanPlayer, volcapPlayer);

        StartCoroutine(ComparisionProcedure(10));

    }

    void ShowPostTrainingHint()
    {
        voteButton1.SetActive(false);
        voteButton2.SetActive(false);
        RepeatButton.SetActive(false);

        inTraining = false;
        PostTrainingHint.SetActive(true);
    }

    public void StartStudy()
    {        
        currentComparisionVariants = CreateCompareVariants(comparisionList.comparisionScenes[currentComparisionIndex]);
        StartCoroutine(ShowNextComparision());
    }

    List<compareVariants> CreateCompareVariants(VideoCollection availableVideos)
    {
        List<compareVariants> variants = new List<compareVariants>();
        if (availableVideos.VolCapVideo == null)
            variants = new List<compareVariants> { compareVariants.DepthkitToLivescan};
        else
            variants = new List<compareVariants> { compareVariants.DepthkitToLivescan, compareVariants.LiveScanToVolcap, compareVariants.VolcapToDepthkit };

        variants.Shuffle();

        return variants;
    }

    void ShowRating()
    {
        voteButton1.SetActive(true);
        voteButton2.SetActive(true);

        if (views < 2)
            RepeatButton.SetActive(true);
    }

    public void RatingButtonClickedLeft()
    {
        if (inTraining)
            ShowPostTrainingHint();

        else
        {
            voteButton1.SetActive(false);
            voteButton2.SetActive(false);
            RepeatButton.SetActive(false);
            StartCoroutine(ShowNextComparision());
        }
    }

    public void RatingButtonClickedRight()
    {
        if (inTraining)
            ShowPostTrainingHint();

        else
        {
            voteButton1.SetActive(false);
            voteButton2.SetActive(false);
            RepeatButton.SetActive(false);
            StartCoroutine(ShowNextComparision());
        }
    }

    public void RepeatButtonClicked()
    {
        if (inTraining)
            StartCoroutine(ComparisionProcedure(10));

        else
            StartCoroutine(ComparisionProcedure(currentVideoCollection.lengthInSeconds));

    }

    IEnumerator ShowNextComparision()
    {
        if (currentComparisionVariantIndex >= currentComparisionVariants.Count)
        {
            currentComparisionIndex++;
            currentComparisionVariantIndex = 0;
            currentComparisionVariants = CreateCompareVariants(comparisionList.comparisionScenes[currentComparisionIndex]);

            if (currentComparisionIndex >= comparisionList.comparisionScenes.Count)
            {
                EndStudy();
                yield break;
            }
        }

        currentVideoCollection = comparisionList.comparisionScenes[currentComparisionIndex];
        compareVariants variant = currentComparisionVariants[currentComparisionVariantIndex];
        bool leftRight = Random.Range(0,1) > 0.5f? true: false;


        switch (variant) 
        {
            case compareVariants.DepthkitToLivescan:
                SetDepthkitPlayer(currentVideoCollection.DepthkitMeta, currentVideoCollection.DepthkitPoster, currentVideoCollection.DepthkitVideo);
                liveScanPlayer.GetComponent<GeometrySequencePlayer>().LoadSequence(currentVideoCollection.LiveScanVideo, GeometrySequenceStream.PathType.AbsolutePath);
                if (leftRight)
                {
                    PreparePlayer(depthkitPlayer, liveScanPlayer);
                    currentLeftCandidate = Candidates.Depthkit;
                    currentRightCandidate = Candidates.LiveScan;
                    currentVariant = compareVariants.DepthkitToLivescan;
                }
                else
                {
                    PreparePlayer(liveScanPlayer, depthkitPlayer);
                    currentLeftCandidate = Candidates.LiveScan;
                    currentRightCandidate = Candidates.Depthkit;
                    currentVariant = compareVariants.DepthkitToLivescan;
                }
                break;
            case compareVariants.LiveScanToVolcap:
                liveScanPlayer.GetComponent<GeometrySequencePlayer>().LoadSequence(currentVideoCollection.LiveScanVideo, GeometrySequenceStream.PathType.AbsolutePath);
                volcapPlayer.GetComponent<GeometrySequencePlayer>().LoadSequence(currentVideoCollection.VolCapVideo, GeometrySequenceStream.PathType.AbsolutePath);
                if (leftRight)
                {
                    PreparePlayer(liveScanPlayer, volcapPlayer);
                    currentLeftCandidate = Candidates.LiveScan;
                    currentRightCandidate = Candidates.VolCap;
                    currentVariant = compareVariants.LiveScanToVolcap;
                }
                else
                {
                    PreparePlayer(volcapPlayer, liveScanPlayer);
                    currentLeftCandidate = Candidates.VolCap;
                    currentRightCandidate = Candidates.LiveScan;
                    currentVariant = compareVariants.LiveScanToVolcap;
                }
                break;
            case compareVariants.VolcapToDepthkit:
                volcapPlayer.GetComponent<GeometrySequencePlayer>().LoadSequence(currentVideoCollection.VolCapVideo, GeometrySequenceStream.PathType.AbsolutePath);
                SetDepthkitPlayer(currentVideoCollection.DepthkitMeta, currentVideoCollection.DepthkitPoster, currentVideoCollection.DepthkitVideo);
                if (leftRight)
                {
                    PreparePlayer(depthkitPlayer, volcapPlayer);
                    currentLeftCandidate = Candidates.LiveScan;
                    currentRightCandidate = Candidates.Depthkit;
                    currentVariant = compareVariants.VolcapToDepthkit;
                }
                else
                {
                    PreparePlayer(volcapPlayer, depthkitPlayer);
                    currentLeftCandidate = Candidates.LiveScan;
                    currentRightCandidate = Candidates.Depthkit;
                    currentVariant = compareVariants.VolcapToDepthkit;
                }
                break;
            default: 
                break;
        }

        currentComparisionVariantIndex++;

        StartCoroutine(ComparisionProcedure(currentVideoCollection.lengthInSeconds));
    }

    void SetDepthkitPlayer(TextAsset meta, Texture2D poster, VideoClip video)
    {
        Depthkit.Clip clip = depthkitPlayer.GetComponent<Depthkit.Clip>();
        clip.metadataFile = meta;
        clip.poster = poster;
        depthkitPlayer.GetComponent<VideoPlayer>().clip = video;            
    }

    IEnumerator ComparisionProcedure(float videoLength)
    {
        voteButton1.SetActive(false);
        voteButton2.SetActive(false);
        RepeatButton.SetActive(false);

        ShowCountdown();
        yield return new WaitForSeconds(3);

        yield return new WaitForSeconds(0.01f);

        PlayComparisionFromStart();
        print(videoLength);
        yield return new WaitForSeconds(videoLength);
        StopComparision();
        ShowRating();
    }

    void ShowCountdown()
    {
        Countdown.GetComponent<Countdown>().StartCountdown();
    }

    void EndStudy()
    {

    }

    void PreparePlayer(GameObject player1, GameObject player2)
    {
        views = 0;

        currentPlayer1 = player1;
        currentPlayer2 = player2;

        player1.SetActive(true);
        player2.SetActive(true);

        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;

        player1Pos.x = podest1.position.x;
        player1Pos.z = podest1.position.z;

        player2Pos.x = podest2.position.x;
        player2Pos.z = podest2.position.z;

        player1.transform.position = player1Pos;
        player2.transform.position = player2Pos;
    }

    public void PlayComparisionFromStart()
    {
        if (views >= 2)
            return;

        views++;

        if (currentPlayer1.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer1.GetComponent<VideoPlayer>().Stop();
            currentPlayer1.GetComponent<VideoPlayer>().Play();
        }

        else
        {
            currentPlayer1.GetComponent<GeometrySequencePlayer>().PlayFromStart();
        }

        if (currentPlayer2.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer2.GetComponent<VideoPlayer>().Play();
        }

        else
        {
            currentPlayer2.GetComponent<GeometrySequencePlayer>().PlayFromStart();
        }

    }

    void StopComparision()
    {
        if (currentPlayer1.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer1.GetComponent<VideoPlayer>().Stop();
        }

        else
        {
            currentPlayer1.GetComponent<GeometrySequencePlayer>().Pause();
        }

        if (currentPlayer2.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer2.GetComponent<VideoPlayer>().Stop();
        }

        else
        {
            currentPlayer2.GetComponent<GeometrySequencePlayer>().Pause();
        }

        currentPlayer1.SetActive(false);
        currentPlayer2.SetActive(false);
    }


}

public static class Extensions
{

    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
