using System.Collections;
using System.Collections.Generic;
using TiltFiveUtils;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(TiltFiveUtils.PlayerIdent))]
public class QuitController : MonoBehaviour
{
    public float animationDurationSeconds = 0.5f;

    public GameObject thingToDisable;

    public TiltFive.GameBoard quitGameboard;

    public UnityEngine.EventSystems.EventSystem eventSystem;

    private TiltFiveUtils.PlayerIdent _playerIdent;

    private TiltFive.PlayerSettings playerSettings;

    private TiltFive.GameBoard _priorGameboard;
    private float _scaleFactor = 1.0f;
    private bool _showQuitMenu = false;

    private float _lerpAmount = 0.0f;

    private Transform _sceneRoot = null;

    // Start is called before the first frame update
    void Awake()
    {
        _playerIdent = GetComponent<TiltFiveUtils.PlayerIdent>();
        TiltFive.Player.TryGetSettings(_playerIdent.playerIndex, out playerSettings);
    }

    public void QuitMenuTogglePerformed(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _showQuitMenu = !_showQuitMenu;

            if (!quitGameboard.isActiveAndEnabled)
            {
                if (_priorGameboard == null)
                {
                    _priorGameboard = playerSettings.gameboardSettings.currentGameBoard;
                }

                // Note that ComputeScaleFactor needs to be called before changing currentGameBoard because it relies
                // on computing the scale from meters to Unity units.
                _scaleFactor = ComputeScaleFactor(_priorGameboard.transform);

                playerSettings.gameboardSettings.currentGameBoard = quitGameboard;

                quitGameboard.gameObject.SetActive(true);
                eventSystem.SetSelectedGameObject(null);
                eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);

                if (thingToDisable != null)
                {
                    thingToDisable.SetActive(false);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (quitGameboard.isActiveAndEnabled)
        {
            if (_showQuitMenu && _lerpAmount < 1.0f)
            {
                _lerpAmount = Mathf.Min(1.0f, _lerpAmount + Time.deltaTime / animationDurationSeconds);
            }
            else if (!_showQuitMenu && _lerpAmount > 0.0f)
            {
                _lerpAmount = Mathf.Max(0.0f, _lerpAmount - Time.deltaTime / animationDurationSeconds);

                if (_lerpAmount == 0.0f)
                {
                    playerSettings.gameboardSettings.currentGameBoard = _priorGameboard;
                    _priorGameboard = null;

                    quitGameboard.gameObject.SetActive(false);

                    if (thingToDisable != null)
                    {
                        thingToDisable.SetActive(true);
                    }
                }
            }
        }

        if (quitGameboard.isActiveAndEnabled)
        {
            var fromTransform = TiltFiveUtils.TransformLocalValue.FromTransform(_priorGameboard.transform);
            var toTransform = fromTransform;
            toTransform.localScale *= _scaleFactor;
            TiltFiveUtils.TransformLocalValue
                .Lerp(fromTransform, toTransform, TiltFiveUtils.EaseUtils.EaseInOut(_lerpAmount))
                .Apply(quitGameboard.transform);
        }
    }

    public void UpdateSceneRoot()
    {
        _sceneRoot = GameObject.Find("Scene")?.transform;
    }

    public void OnCancelQuitMenu()
    {
        _showQuitMenu = false;
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private float ComputeScaleFactor(Transform gameboard)
    {
        float scaleFactor = 1.0f;

        GameObject scene = GameObject.Find("Scene");
        if (scene != null)
        {
            const float TARGET_MAX_EXTENT_METERS = 0.15f; // 15 cm
            float targetMaxDistanceFromGameboardCenter = TiltFiveUtils.PlayerUtils.GetScaleToUnityFromMeters(_playerIdent.playerIndex)
                * TARGET_MAX_EXTENT_METERS;
            Bounds bounds = RecursiveRenderBounds(scene);
            BoundingSphere boundingSphere = ToBoundingSphere(bounds);
            float maxDistanceFromGameboardCenter = (boundingSphere.position - gameboard.position).magnitude + boundingSphere.radius;
            scaleFactor = Mathf.Max(maxDistanceFromGameboardCenter / targetMaxDistanceFromGameboardCenter, 2.0f);
        }

        Debug.Log("scaleFactor: " + scaleFactor.ToString());

        return scaleFactor;
    }

    private static Bounds RecursiveRenderBounds(GameObject obj)
    {
        Bounds bounds;
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            bounds = renderer.bounds;
        }
        else
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
        }

        foreach (Transform child in obj.transform)
        {
            bounds.Encapsulate(RecursiveRenderBounds(child.gameObject));
        }

        return bounds;
    }

    private static BoundingSphere ToBoundingSphere(Bounds bounds)
    {
        return new BoundingSphere(bounds.center, bounds.extents.magnitude);
    }
}
