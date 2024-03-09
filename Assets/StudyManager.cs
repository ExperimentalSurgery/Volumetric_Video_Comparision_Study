using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using BuildingVolumes.Streaming;
using System;
using System.IO;

public class StudyManager : MonoBehaviour
{
    public enum compareVariants
    {
        DepthkitToLivescan,
        LiveScanToDepthkit,
        LiveScanToVolcap,
        VolcapToLiveScan,
        VolcapToDepthkit,
        DepthkitToVolcap
    }

    public enum Candidates
    {
        Depthkit,
        LiveScan,
        VolCap,
        none
    }

    public enum Sequences
    {
        staticHuman,
        dynamicHuman,
        objectHuman,
        hands,
        jenga

    }

    public StudyComparisions comparisionList;
    List<compareVariants> currentComparisionVariants;

    public Vector3 podest1Offset;
    public Vector3 podest2Offset;

    public GameObject depthkitPlayer;
    public GameObject liveScanPlayer;
    public GameObject volcapPlayer;

    GameObject currentPlayer1;
    GameObject currentPlayer2;
    Candidates currentLeftCandidate;
    Candidates currentRightCandidate;
    compareVariants currentVariant;
    Sequences currentSequence;

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
    public Transform trainingTransGood;
    public Transform trainingTransBad;

    VideoCollection currentVideoCollection;
    ComparisionVotes comparisionVotes;

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

        comparisionVotes = new ComparisionVotes();
    }

    public void StartTraining()
    {
        inTraining = true;

        liveScanPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(trainingVideoBad, GeometrySequenceStream.PathType.AbsolutePath);
        volcapPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(trainingVideoGood, GeometrySequenceStream.PathType.AbsolutePath);

        PreparePlayer(liveScanPlayer, volcapPlayer, trainingTransGood, trainingTransBad);

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
        if (availableVideos.VolCapVideo.Length < 1)
            variants = new List<compareVariants> { compareVariants.DepthkitToLivescan, compareVariants.LiveScanToDepthkit };
        else
            variants = new List<compareVariants> { compareVariants.DepthkitToLivescan, compareVariants.LiveScanToDepthkit, compareVariants.LiveScanToVolcap, compareVariants.VolcapToLiveScan, compareVariants.VolcapToDepthkit, compareVariants.DepthkitToVolcap };

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
        print("Voted for: " + currentLeftCandidate.ToString());
        RecordVote(currentVariant, currentLeftCandidate, currentSequence, comparisionVotes);
        string json = JsonUtility.ToJson(comparisionVotes);

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
        print("Voted for: " + currentRightCandidate.ToString());
        RecordVote(currentVariant, currentRightCandidate, currentSequence, comparisionVotes);


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

            if (currentComparisionIndex >= comparisionList.comparisionScenes.Count)
            {
                EndStudy();
                yield break;
            }

            currentComparisionVariantIndex = 0;
            currentComparisionVariants = CreateCompareVariants(comparisionList.comparisionScenes[currentComparisionIndex]);
        }

        currentVideoCollection = comparisionList.comparisionScenes[currentComparisionIndex];
        compareVariants variant = currentComparisionVariants[currentComparisionVariantIndex];
        currentSequence = comparisionList.comparisionScenes[currentComparisionIndex].sequence;

        liveScanPlayer.GetComponent<GeometrySequencePlayer>().Hide();
        volcapPlayer.GetComponent<GeometrySequencePlayer>().Hide();
        depthkitPlayer.GetComponent<Depthkit.StudioLook>().enabled = false;


        switch (variant)
        {
            case compareVariants.DepthkitToLivescan:
                SetDepthkitPlayer(currentVideoCollection.DepthkitMeta, currentVideoCollection.DepthkitPoster, currentVideoCollection.DepthkitVideo);
                liveScanPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.LiveScanVideo, GeometrySequenceStream.PathType.AbsolutePath);

                PreparePlayer(depthkitPlayer, liveScanPlayer, currentVideoCollection.DepthkitTransform, currentVideoCollection.LiveScanTransform);
                currentLeftCandidate = Candidates.Depthkit;
                currentRightCandidate = Candidates.LiveScan;
                currentVariant = compareVariants.DepthkitToLivescan;
                print("Comparing Depthkit (left) to Livescan (right)");
                break;
            case compareVariants.LiveScanToDepthkit:
                SetDepthkitPlayer(currentVideoCollection.DepthkitMeta, currentVideoCollection.DepthkitPoster, currentVideoCollection.DepthkitVideo);
                liveScanPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.LiveScanVideo, GeometrySequenceStream.PathType.AbsolutePath);

                PreparePlayer(liveScanPlayer, depthkitPlayer, currentVideoCollection.LiveScanTransform, currentVideoCollection.DepthkitTransform);
                currentLeftCandidate = Candidates.LiveScan;
                currentRightCandidate = Candidates.Depthkit;
                currentVariant = compareVariants.LiveScanToDepthkit;
                print("Comparing Livescan (left) to Depthkit (right) ");
                break;
            case compareVariants.LiveScanToVolcap:
                liveScanPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.LiveScanVideo, GeometrySequenceStream.PathType.AbsolutePath);
                volcapPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.VolCapVideo, GeometrySequenceStream.PathType.AbsolutePath);

                PreparePlayer(liveScanPlayer, volcapPlayer, currentVideoCollection.LiveScanTransform, currentVideoCollection.VolcapTransform);
                currentLeftCandidate = Candidates.LiveScan;
                currentRightCandidate = Candidates.VolCap;
                currentVariant = compareVariants.LiveScanToVolcap;
                print("Comparing LiveScan (left) to Volcap (right)");
                break;
            case compareVariants.VolcapToLiveScan:
                liveScanPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.LiveScanVideo, GeometrySequenceStream.PathType.AbsolutePath);
                volcapPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.VolCapVideo, GeometrySequenceStream.PathType.AbsolutePath);

                PreparePlayer(volcapPlayer, liveScanPlayer, currentVideoCollection.VolcapTransform, currentVideoCollection.LiveScanTransform);
                currentLeftCandidate = Candidates.VolCap;
                currentRightCandidate = Candidates.LiveScan;
                currentVariant = compareVariants.VolcapToLiveScan;
                print("Comparing Volcap (left) to Livescan (right)");
                break;
            case compareVariants.DepthkitToVolcap:
                volcapPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.VolCapVideo, GeometrySequenceStream.PathType.AbsolutePath);
                SetDepthkitPlayer(currentVideoCollection.DepthkitMeta, currentVideoCollection.DepthkitPoster, currentVideoCollection.DepthkitVideo);

                PreparePlayer(depthkitPlayer, volcapPlayer, currentVideoCollection.DepthkitTransform, currentVideoCollection.VolcapTransform);
                currentLeftCandidate = Candidates.Depthkit;
                currentRightCandidate = Candidates.VolCap;
                currentVariant = compareVariants.DepthkitToVolcap;
                print("Comparing Depthkit (left) to Volcap (right)");
                break;
            case compareVariants.VolcapToDepthkit:
                volcapPlayer.GetComponent<GeometrySequencePlayer>().OpenSequence(currentVideoCollection.VolCapVideo, GeometrySequenceStream.PathType.AbsolutePath);
                SetDepthkitPlayer(currentVideoCollection.DepthkitMeta, currentVideoCollection.DepthkitPoster, currentVideoCollection.DepthkitVideo);

                PreparePlayer(volcapPlayer, depthkitPlayer, currentVideoCollection.VolcapTransform, currentVideoCollection.DepthkitTransform);
                currentLeftCandidate = Candidates.VolCap;
                currentRightCandidate = Candidates.Depthkit;
                currentVariant = compareVariants.VolcapToDepthkit;
                print("Comparing Volcap (left) to Depthkit (right)");
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
        EndStudyHint.SetActive(true);
        string json = JsonUtility.ToJson(comparisionVotes);
        string guid = Guid.NewGuid().ToString();

        System.IO.File.WriteAllText(Application.dataPath + "\\..\\..\\Results\\" + guid + ".json", json);
    }

    void PreparePlayer(GameObject player1, GameObject player2, Transform posPlayer1, Transform posPlayer2)
    {
        views = 0;

        currentPlayer1 = player1;
        currentPlayer2 = player2;

        player1.transform.position = posPlayer1.position;
        player1.transform.rotation = posPlayer1.rotation;
        player1.transform.localScale = posPlayer1.localScale;
        player1.transform.position += podest1Offset;

        player2.transform.position = posPlayer2.position;
        player2.transform.rotation = posPlayer2.rotation;
        player2.transform.localScale = posPlayer2.localScale;
        player2.transform.position += podest2Offset;

        Debug.Log("Player prepared!");
    }

    public void PlayComparisionFromStart()
    {
        if (views >= 2)
            return;

        views++;

        if (currentPlayer1.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer1.GetComponent<Depthkit.StudioLook>().enabled = true;
            currentPlayer1.GetComponent<VideoPlayer>().Stop();
            currentPlayer1.GetComponent<VideoPlayer>().Play();
        }

        else
        {
            currentPlayer1.GetComponent<GeometrySequencePlayer>().PlayFromStart();
            currentPlayer1.GetComponent<GeometrySequencePlayer>().Show();
        }

        if (currentPlayer2.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer2.GetComponent<Depthkit.StudioLook>().enabled = true;
            currentPlayer2.GetComponent<VideoPlayer>().Stop();
            currentPlayer2.GetComponent<VideoPlayer>().Play();
        }

        else
        {
            currentPlayer2.GetComponent<GeometrySequencePlayer>().PlayFromStart();
            currentPlayer2.GetComponent<GeometrySequencePlayer>().Show();
        }

    }

    void StopComparision()
    {
        if (currentPlayer1.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer1.GetComponent<VideoPlayer>().Stop();
            currentPlayer1.GetComponent<Depthkit.StudioLook>().enabled = false;
        }

        else
        {
            currentPlayer1.GetComponent<GeometrySequencePlayer>().Pause();
            currentPlayer1.GetComponent<GeometrySequencePlayer>().Hide();
        }

        if (currentPlayer2.GetComponent<VideoPlayer>() != null)
        {
            currentPlayer2.GetComponent<VideoPlayer>().Stop();
            currentPlayer2.GetComponent<Depthkit.StudioLook>().enabled = false;
        }

        else
        {
            currentPlayer2.GetComponent<GeometrySequencePlayer>().Pause();
            currentPlayer2.GetComponent<GeometrySequencePlayer>().Hide();
        }
    }

    void RecordVote(compareVariants variant, Candidates choosenCandidate, Sequences sequence, ComparisionVotes votes)
    {
        comparisionVotes.Votes.Add(new Vote(sequence, variant, choosenCandidate));
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

[Serializable]
public class ComparisionVotes
{
    public List<Vote> Votes = new List<Vote>();
}

[Serializable]
public class Vote
{
    public StudyManager.Sequences sequence;
    public StudyManager.compareVariants variant;
    public StudyManager.Candidates votedForCandidate;

    public Vote(StudyManager.Sequences sequence, StudyManager.compareVariants variant, StudyManager.Candidates votedFor)
    {
        this.sequence = sequence;
        this.variant = variant;
        this.votedForCandidate = votedFor;

    }
    
}

