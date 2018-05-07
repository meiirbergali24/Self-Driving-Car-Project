using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;

public class WeAreShootingStar : MonoBehaviour {

    [SerializeField]
    private MeshRenderer mesh;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Material toBizaaare;
    [SerializeField]
    private List<Light> lights;
    [SerializeField]
    private new CarFollowingCamera camera;
    [SerializeField]
    private float TIMETOFIRE = 15f;
    [SerializeField, Space]
    private Material spaceSkyBox;
    [SerializeField]
    private Material defaultSkyBox;
    private new AudioSource audio;

    private readonly Color[] TUTUTUTUTURU = new Color[] { Color.red, Color.blue, Color.green};
        
    private void Start()
    {
        audio = GetComponent<AudioSource>();
        StartCoroutine(WEARE());
    }

    private void OnApplicationQuit()
    {
        RenderSettings.skybox = defaultSkyBox;
        toBizaaare.SetColor("_EmissionColor",Color.black);
    }

    private IEnumerator WEARE()
    {
        bool NOW = false;
        float time = 0;
        Vector3 offset = camera.Offset;
        float period = 15f;
        int flip = 1;
        
        SHOOOTINGSTAR:

        if (!NOW && (
            mesh.bounds.max.x < target.position.x ||
            mesh.bounds.min.x > target.position.x ||
            mesh.bounds.min.z > target.position.x ||
            mesh.bounds.max.z < target.position.x))
        {
            RenderSettings.skybox = spaceSkyBox;
            NOW = true;
            time = 0;
            //Debug.Log("SHIT" + time);
        }

        if (NOW)
        {

            time += Time.deltaTime;
            //Debug.Log($"{time} {Time.deltaTime}" );
            if (time > 1f)
            {
                var temp = TUTUTUTUTURU[2];
                TUTUTUTUTURU[2] = TUTUTUTUTURU[1];
                TUTUTUTUTURU[1] = TUTUTUTUTURU[0];
                TUTUTUTUTURU[0] = temp;
                time = 0;
                //Debug.Log($"{TUTUTUTUTURU[0]} {TUTUTUTUTURU[1]}");
            }

            float gradus = Mathf.Cos(Mathf.PingPong(Time.time, 1)) * flip;

            period -= flip * Time.deltaTime;
            if (Mathf.Abs(period) < flip)
            {
                flip = -flip;
                period = flip * 15f;
            }

            camera.Offset = new Vector3(
                gradus * Mathf.Clamp(offset.x,1, 42) * flip,
                gradus * Mathf.Clamp(offset.y, 1, 42) * flip, 
                gradus * Mathf.Clamp(offset.z, 1, 42) * flip);
                
            if (TIMETOFIRE < 0)
            {
                ///TUTUTUTUTUTEURURURURUTUTUTUTUTURUURURURTUTUTUTUTUURUURURURUTUTUTUTU
                foreach (var light in lights)
                {
                    light.intensity = time;
                }
                toBizaaare.SetColor("_EmissionColor", Color.Lerp(TUTUTUTUTURU[0], TUTUTUTUTURU[1], time));
            }
            else
            {
                TIMETOFIRE -= Time.deltaTime;
            }

            //if(Time.frameCount % 20 == 0) Debug.Log($"SHOOTING STAR!! {Color.Lerp(TUTUTUTUTURU[0], TUTUTUTUTURU[1], time)}");

            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }

        yield return null;
        goto SHOOOTINGSTAR;
    }
}
