using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FxObject : MonoBehaviour
{
    protected FxManager.FxPool _pool;
    public LinkedListNode<FxObject> Node { get; protected set; }

    [SerializeField]
    private bool isLoop = false;
    
    [Header("Particle")]
    [SerializeField]
    private ParticleSystem[] _particles;
    [SerializeField]
    private ParticleSystem[] _particlesForLoop;

    [Header("Animation (Legacy)")]
    [SerializeField]
    protected Animation _anim = null;    
    [SerializeField]
    protected string _animName_Start = null;
    [SerializeField]
    protected string _animName_Loop = null;
    [SerializeField]
    protected string _animName_End = null;
    
    [SerializeField][HideInInspector]
    protected bool _hasStartAnim;
    [SerializeField][HideInInspector]
    protected bool _hasLoopAnim;
    [SerializeField][HideInInspector]
    protected bool _hasEndAnim;
    
    [Header("Trail")]
    [SerializeField]
    protected TrailRendererEx[] _trails;
    [SerializeField]
    protected TrailRendererEx[] _trailsForLoop;
    
    private bool _waitForEnd;

    private System.Action<FxObject> _endCallback;
    
    protected virtual void Awake()
    {
        Node = new(this);
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
    }

    protected virtual void OnDestroy()
    {
        if (Node != null)
        {
            Node.RemoveSelf();
            Node.Value = null;
            Node = null;
        }
    }
    
    public void Init(FxManager.FxPool master)
    {
        _pool = master;
    }
    
    public void Play(System.Action<FxObject> endCallback)
    {
        _endCallback = endCallback;
        
        /* Play :: Particle */
        if (_particles != null)
            _particles.ForEach(particle => particle.Play(false));
        
        // Play :: Anim
        if (_anim != null)
        {
            _anim.Stop();

            if (_hasStartAnim)
            {
                var animState = _anim.PlayQueued(_animName_Start);
                if (!_hasLoopAnim && _hasEndAnim)
                    animState.wrapMode = WrapMode.ClampForever;
                else
                    animState.wrapMode = WrapMode.Clamp;
            }

            if (_hasLoopAnim)
            {
                var animState = _anim.PlayQueued(_animName_Loop, QueueMode.CompleteOthers);
                animState.wrapMode = WrapMode.Loop;
            }
        }

        // Play :: Trail
        if (_trails != null)
            _trails.ForEach(trail => trail.Play());
        
        if (!isLoop)
            ReturnToPool(false);
    }
    
    public void ReturnToPool(bool bForced)
    {
        if (bForced)
        {
            StopAllCoroutines();
            _waitForEnd = false;
            
            if (_pool != null)
                _pool.ReleaseFxObject(this);

            if (_endCallback != null)
            {
                var call = _endCallback;
                _endCallback = null;
                call.Invoke(this);
            }
            
            return;
        }

        if (_waitForEnd)
            return;

        /* Stop :: Particle */
        _particlesForLoop.ForEach(particle => particle.Stop(false, ParticleSystemStopBehavior.StopEmitting));
        
        /* Stop :: Anim */
        if (_hasEndAnim)
        {
            _anim.CrossFade(_animName_End, 0.15f);
            _anim[_animName_End].wrapMode = WrapMode.Clamp;
        }

        var isEndParticle = _particles == null || _particles.Length == 0;
        var isEndAnim = _anim == null || !_anim.isPlaying;
        var isEndTrail = _trails == null || _trails.Length == 0;

        _waitForEnd = true;

        if (!isEndParticle)
        {
            StartCoroutine(_Cor_WaitForEnd_Particle());
            IEnumerator _Cor_WaitForEnd_Particle()
            {
                bool _IsPaticlesAlived()
                {
                    foreach (var particle in _particles)
                    {
                        if (particle.IsAlive(false))
                            return true;
                    }

                    return false;
                }
                
                while (_IsPaticlesAlived())
                    yield return null;

                isEndParticle = true;
                if (isEndParticle && isEndAnim && isEndTrail)
                    ReturnToPool(true);
            }
        }

        if (!isEndAnim)
        {
            StartCoroutine(_Cor_WaitForEnd_Anim());
            IEnumerator _Cor_WaitForEnd_Anim()
            {
                while (_anim.isPlaying)
                    yield return null;

                isEndAnim = true;
                if (isEndParticle && isEndAnim && isEndTrail)
                    ReturnToPool(true);
            }
        }
        
        if (!isEndTrail)
        {
            StartCoroutine(_Cor_WaitForEnd_Trail());
            IEnumerator _Cor_WaitForEnd_Trail()
            {
                bool _IsTrailAlived()
                {
                    foreach (var trail in _trails)
                    {
                        if (trail.IsAlive())
                            return true;
                    }

                    return false;
                }

                while (_IsTrailAlived())
                    yield return null;
                
                isEndTrail = true;
                if (isEndParticle && isEndAnim && isEndTrail)
                    ReturnToPool(true);
            }
        }
    }
    
    public void UpdateComponents()
    {
        /* Particle */
        _particles = GetComponentsInChildren<ParticleSystem>(true);
        _particlesForLoop = _particles.Where(x => x.main.loop).ToArray();

        /* Animation */
        _anim = GetComponent<Animation>();
        if (_anim == null)
        {
            _hasStartAnim = false; 
            _hasLoopAnim = false;
            _hasEndAnim = false;
        }
        else
        {
            _anim.playAutomatically = false;
            _hasStartAnim = !string.IsNullOrWhiteSpace(_animName_Start) && _anim.GetClip(_animName_Start) != null;
            _hasLoopAnim = !string.IsNullOrWhiteSpace(_animName_Loop) && _anim.GetClip(_animName_Loop) != null;
            _hasEndAnim = !string.IsNullOrWhiteSpace(_animName_End) && _anim.GetClip(_animName_End) != null;
        }
        
        /* Trail */
        _trails = GetComponentsInChildren<TrailRendererEx>(true);
        _trailsForLoop = _trails.Where(x => x.IsLoop).ToArray();
        
        isLoop = _particlesForLoop.Length > 0 || _hasLoopAnim || _hasEndAnim || _trailsForLoop.Length > 0;
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}