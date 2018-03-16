using MindTrick;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    [SerializeField] private Animator _animator;

	// Use this for initialization
	void Start () {
        new Timer(0.2f, delegate { _animator.SetTrigger("Show"); }).Play();
        
	}
	
	public void CarPressed()
    {
        ModuleManager.ForEachModule(delegate (Module module) { module.Cancel(); });
        SceneManager.LoadScene(2);
    }

    public void MapPressed()
    {
        ModuleManager.ForEachModule(delegate (Module module) { module.Cancel(); });
        SceneManager.LoadScene(1);
    }
}
