using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class StudyComparisions : MonoBehaviour
{
    public List<VideoCollection> comparisionScenes = new List<VideoCollection>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class VideoCollection
{
    public string name;
    public float lengthInSeconds;
    public TextAsset DepthkitMeta;
    public Texture2D DepthkitPoster;
    public VideoClip DepthkitVideo;
    public string LiveScanVideo;
    public string VolCapVideo;
    public Transform DepthkitTransform;
    public Transform LiveScanTransform;
    public Transform VolcapTransform;
}
