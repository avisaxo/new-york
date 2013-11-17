﻿using UnityEngine;
using System.Collections;

public class ItemScreen : ApplicationScreen {

	public override void Activate(bool immediate, int startPoint){
		base.Activate(immediate, startPoint);
		gameObject.SetActive(true);
		for(int i=0; i<4; i++){
			
				
				custom[i].Transparency = 0;

		}
		if(immediate){
			uiTransform.RelativePosition = new Vector2(0, 0);
			//SetCameraX(0);
			animating = false;
		}
		else{
			time = 0;
			uiTransform.RelativePosition = new Vector2(1, 0);
			xStart = startPoint;
			xTarget = 0;
			animating = true;
			activating = true;
		}
	}

	public override void Deactivate(bool immediate, int endPoint){
		base.Deactivate(immediate, endPoint);
		if(immediate){
			uiTransform.RelativePosition = new Vector2(0, 0);
			gameObject.SetActive(false);
			animating = false;
		}
		else{
			time = 0;
			xStart = 0;
			xTarget = endPoint;
			animating = true;
			activating = false;
		}
	}

	// void SetCameraX(float val){
	// 	Rect r = customizerCamera.pixelRect;
	// 		r.x = val;
	// 		r.width = Screen.width;
	// 		r.height = Screen.height*0.5f;
	// 		r.y = 162 /1136f;
	// 		customizerCamera.pixelRect = r;
	// }
	const string PARAMETER_URL = Config.BASE_URL+"/parameters.fetch.php";

	float xStart;
	float xTarget;
	float time;
	bool animating = false;
	bool activating = false;

	[SerializeField] UITapGestureRecognizer buy;
	[SerializeField] UITapGestureRecognizer back;
	[SerializeField] UIScrollGestureRecognizer scroller;
	[SerializeField] Camera customizerCamera;
	[SerializeField] Transform cameraParent;
	[SerializeField] UITextureNode touchArea;
	[SerializeField] UITextNode title;
	[SerializeField] UITextNode description;
	[SerializeField] UITextNode merchant;
	[SerializeField] UITextureNode icon;

	[SerializeField] UITextNode parameterOne;
	[SerializeField] UITextNode parameterTwo;
	[SerializeField] UITextNode parameterThree;

	[SerializeField] UITextNode[] custom;
	[SerializeField] AttributeScroll[] scrollers;

	[SerializeField] GameObject[] objCache;

	int selectedParameter = 0;

	float targetYaw;
	float targetPitch;
	float pitch;
	float pitchVelocity;
	float yaw;
	float yawVelocity;

	int item = 0;
	ItemEntity entity;
	public void SetItem(int item){
		icon.Texture = null;
		this.item = item;
		entity = null;
		selectedParameter = 0;
	}

	void Start(){
		
		back.OnGestureRecognized += Home;
		scroller.OnGestureChanged += Scroll;
		buy.OnGestureRecognized += Buy;
	}

	void Buy(int idx){
		if(entity != null){
			StateMachine.Instance.Checkout(1);
		}
	}

	void Scroll(int idx){
		targetYaw = (targetYaw + 0.4f*scroller.RawScrollAmount.x);
		targetPitch = Mathf.Clamp(targetPitch + 0.3f*scroller.RawScrollAmount.y,-89,89);

		//invalidate button presses
		downParam = -1;

	}

	void Home(int idx){
		
		StateMachine.Instance.GotoHomeScreen(-1);

	}

	int downParam =-1;

	void Update(){
		if(activated){
			if(entity == null){
				entity = ItemManagement.Instance.GetItem(item);
				if(entity != null){
					DrawItem();
				}
			}
			else{
				UpdateScrollers();
				if(Input.GetKey(KeyCode.Mouse0)){
					Ray ray = customizerCamera.ScreenPointToRay(Input.mousePosition);
					 RaycastHit hit;
				    if(Physics.Raycast(ray, out hit, 100)){
				        string name = hit.collider.name;
				        for(int i=0; i<entity.Parameters.Count; i++){
				        	if(name == entity.Parameters[i].name){
				        		if(i < 4){
				        			downParam = i;
				        			
				        			// hit.collider.gameObject.renderer.material.color = new Color(1,1,0.5f,1);
				        		}
				        		
				        		
				        	}
				        }
				        
				    }
				}
				else{
					if(downParam != -1){

						selectedParameter = downParam;
	        			if(objCache[entity.Id] != null){
	        				SetObjectColors(objCache[entity.Id].transform);
	        			}
	        			downParam = -1;
					}
					
				}
				 if(icon.Texture == null){
					icon.Texture = PhotoManager.Instance.GetImage(entity.Id);
				}
			}

			


		}
		if(animating){
			time = Mathf.Clamp01(2f*Time.deltaTime + time);
			float val = Smoothing.QuinticEaseOut(time);
			val = val*xTarget + (1-val)*xStart;
			uiTransform.RelativePosition = new Vector2(val, 0);
			//SetCameraX(Screen.width*val);
			if(time == 1){
				animating = false;
				if(!activating){
					gameObject.SetActive(false);
				}
			}
		}
		pitch = Smoothing.SpringSmooth(pitch, targetPitch, ref pitchVelocity, 0.2f, Time.deltaTime);
		float tYaw = Mathf.DeltaAngle(yaw, targetYaw);
		yaw = Smoothing.SpringSmooth(yaw, targetYaw, ref yawVelocity, 0.2f, Time.deltaTime);
		cameraParent.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
	}

	void InitializeScrollers(){
		
	}

	void UpdateScrollers(){
		if(objCache[entity.Id] != null){
			Transform t = GetSelectedParameter(objCache[entity.Id].transform);
			if(t != null){
			int idx =0;
			//Vector3 position = t.position;
			//Vector3 scale = t.localScale;
			if(entity.Parameters[selectedParameter].translate_x){
				
				float x = (1-scrollers[idx].Value)*entity.Parameters[selectedParameter].translate_x_min + scrollers[idx].Value*entity.Parameters[selectedParameter].translate_x_max;
				t.position = new Vector3(x,t.position.y, t.position.z);
				//custom[idx].Text = "Move X";
				idx++;
			}
			if(entity.Parameters[selectedParameter].translate_y){
				float y = (1-scrollers[idx].Value)*entity.Parameters[selectedParameter].translate_y_min + scrollers[idx].Value*entity.Parameters[selectedParameter].translate_y_max;
				t.position = new Vector3(t.position.x,y, t.position.z);
				idx++;
			}
			if(entity.Parameters[selectedParameter].translate_z){
				float z = (1-scrollers[idx].Value)*entity.Parameters[selectedParameter].translate_z_min + scrollers[idx].Value*entity.Parameters[selectedParameter].translate_z_max;
				t.position = new Vector3(t.position.x, t.position.y,z);
				idx++;
			}

			if(entity.Parameters[selectedParameter].rotate_x){
				//custom[idx].Text = "Rotate X";
				idx++;
			}
			if(entity.Parameters[selectedParameter].rotate_y){
				//custom[idx].Text = "Rotate Y";
				idx++;
			}
			if(entity.Parameters[selectedParameter].rotate_z){
				//custom[idx].Text = "Rotate Z";
				idx++;
			}

			if(entity.Parameters[selectedParameter].scale_x){
				float x = (1-scrollers[idx].Value)*entity.Parameters[selectedParameter].scale_x_min + scrollers[idx].Value*entity.Parameters[selectedParameter].scale_x_max;
				t.localScale = new Vector3(x, t.localScale.y, t.localScale.z);
				idx++;
			}
			if(entity.Parameters[selectedParameter].scale_y){
				float y = (1-scrollers[idx].Value)*entity.Parameters[selectedParameter].scale_y_min + scrollers[idx].Value*entity.Parameters[selectedParameter].scale_y_max;
				t.localScale = new Vector3(t.localScale.x, y, t.localScale.z);
				idx++;
			}
			if(entity.Parameters[selectedParameter].scale_z){
				float z = (1-scrollers[idx].Value)*entity.Parameters[selectedParameter].scale_z_min + scrollers[idx].Value*entity.Parameters[selectedParameter].scale_z_max;
				t.localScale = new Vector3(t.localScale.x, t.localScale.y,z);
				idx++;
			}
		}
		}
	}


	Transform GetSelectedParameter(Transform trans){
		foreach(Transform t in trans){
			// Debug.Log(t.name + " : " + entity.Parameters[selectedParameter].name);
			if(t.name == entity.Parameters[selectedParameter].name){

				return t;
			}
			else{
				Transform tr =  GetSelectedParameter(t);
				if(tr != null){
					return tr;
				}
			}
		}
		return null;
	}


	Color baseColor = new Color(1,1,1,1);
	Color selectColor = new Color(0.7f,0.68f,1,1);
	void SetObjectColors(Transform trans){
		foreach(Transform t in trans){
			if(t.name == entity.Parameters[selectedParameter].name){
				t.gameObject.renderer.material.color = selectColor;
				// Debug.Log(t.name);
			}
			else{
				// Debug.Log("norm" + t.name);
				t.gameObject.renderer.material.color = baseColor;
			}
			SetObjectColors(t);
		}
		SetupSliders();
	}

	void SetupSliders(){
		int idx =0;
		if(entity.Parameters[selectedParameter].translate_x){
			custom[idx].Text = "Move X";
			idx++;
		}
		if(entity.Parameters[selectedParameter].translate_y){
			custom[idx].Text = "Move Y";
			idx++;
		}
		if(entity.Parameters[selectedParameter].translate_z){
			custom[idx].Text = "Move Z";
			idx++;
		}

		if(entity.Parameters[selectedParameter].rotate_x){
			custom[idx].Text = "Rotate X";
			idx++;
		}
		if(entity.Parameters[selectedParameter].rotate_y){
			custom[idx].Text = "Rotate Y";
			idx++;
		}
		if(entity.Parameters[selectedParameter].rotate_z){
			custom[idx].Text = "Rotate Z";
			idx++;
		}

		if(entity.Parameters[selectedParameter].scale_x){
			custom[idx].Text = "Scale X";
			idx++;
		}
		if(entity.Parameters[selectedParameter].scale_y){
			custom[idx].Text = "Scale Y";
			idx++;
		}
		if(entity.Parameters[selectedParameter].scale_z){
			custom[idx].Text = "Scale Z";
			idx++;
		}
		// Debug.Log(idx);
		for(int i=0; i<4; i++){
			if(idx <= i){
				// Debug.Log(custom[idx].name);
				custom[i].Transparency = 0;
			}
			else{
				custom[i].Transparency = 1;
			}
		}
	}

	void DrawItem(){
		title.Text = entity.Name;
		description.Text = entity.Description;
		merchant.Text = entity.MerchantName;
		PhotoManager.Instance.LoadImage(entity.Id, entity.PhotoLargeURL);

		//oh god haxxxxx
		if(entity.Id < objCache.Length){
			objCache[entity.Id].SetActive(true);
		}
		for(int i=0; i<objCache.Length; i++){
			if(i != entity.Id){
				if(objCache[i] != null){
					objCache[i].SetActive(false);
				}
			}
		}

		for(int i=0; i<entity.Parameters.Count; i++){
			if(i==0){
				parameterOne.gameObject.SetActive(true);
				parameterOne.Text = entity.Parameters[i].name;
			}
			else if(i==1){
				parameterTwo.gameObject.SetActive(true);
				parameterTwo.Text = entity.Parameters[i].name;
			}
			else if(i==2){
				parameterThree.gameObject.SetActive(true);
				parameterThree.Text = entity.Parameters[i].name;
			}
		}
		if(entity.Parameters.Count < 3){
			parameterThree.gameObject.SetActive(false);
		}
		if(entity.Parameters.Count < 2){
			parameterTwo.gameObject.SetActive(false);
		}
		if(entity.Parameters.Count < 1){
			parameterOne.gameObject.SetActive(false);
		}
		if(objCache[entity.Id] != null){
			SetObjectColors(objCache[entity.Id].transform);
		}
		//StartCoroutine(LoadModel());
		Debug.Log("hell yeah");

		// StartCoroutine(GetParameters());
	}

	IEnumerator LoadModel(){

		WWW www = new WWW( entity.ModelURL );

        yield return www;
        if(www.error != null){
			Debug.LogError("ERROR ON MODEL LOAD: "+ www.error);
		}
		else{
			///Debug.Log(www.data);
			//yikes, we get obj data, but the parser is broken, so we'll hack it... :(


		}

		yield return 0;
	}

}
