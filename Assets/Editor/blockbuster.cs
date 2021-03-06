﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using UnityEditor;
using System.Xml;

public enum ACTIVEBASENAME
{
	HIGHTECH = 0, 
	JUNGLE = 1, 
	TEMPLE = 2,
	SANDBOX = 3
}

// add test comment



public class blockbuster : EditorWindow
{
    bool
        b_fx,
        editsub,
        step,
        //b_triggeronce,
        bb_dirty,
        b_groupselectmode
        ;
    float
        stepvalue
        ;
    int
        i,
        assetbaseindex,
        slideindex
        //bs.paramblock.targetindex=0
        
        ;

    public int toolbarInt ;


    ParameterBlock UIPB = new ParameterBlock();


    private int bsz = 20;
    Vector3 S = new Vector3(0, 0, 0);
    //Vector3 dir = new Vector3(0, 0, 0);

    Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
    Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
    Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
    Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);

    public  Texture2D uparrow   ;//= new Texture2D(20,20)  ;
	public  Texture2D downarrow ;//= new Texture2D(20,20)  ;
	public  Texture2D leftarrow ;//= new Texture2D(20,20)  ;
	public  Texture2D rightarrow ;//= new Texture2D(20,20)  ;
	
	public  Texture2D d_uparrow ;//= new Texture2D(20,20)  ;
	public  Texture2D d_downarrow ;//= new Texture2D(20,20)  ;
	public  Texture2D d_leftarrow ;//= new Texture2D(20,20)  ;
	public  Texture2D d_rightarrow;//= new Texture2D(20,20)  ;
	public  Texture2D duplicate_t ;//= new Texture2D(20,20)  ;

    public ArrayList  hidenobjectlist = new ArrayList() ;

    int oldindexstorage ;
    //string[] toolbarStrings = new String[] { "blockmove", "block props", "tools" };
    //COLIDER_TYPE colider_type;
    
    //PLTF_TYPE pltf_sate;

    blocksetup bs ;

    ACTIVEBASENAME activebasename;
    string selectedbasename = "/PLATFORM/HIGHTECH/";
    public List<string> data;

    //static GameObject handle = (GameObject)Resources.LoadAssetAtPath(("Assets/Editor/target.fbx"), typeof(GameObject));


    [MenuItem("Window/blockbuster")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(blockbuster));
    }






    void Start()
    {
        InitGUIValues(); 
    }


    public bool savescene(bool preset = false , string scenename=null)
    {
        
        string repo ="";
        string defname = ""; 
        Scene scenetosave = new Scene();
        blocksetup tbs;
        GameObject go;
        object[] obj ;
        if (!preset)
        {
            obj = GameObject.FindObjectsOfType(typeof(GameObject));
            repo = "scenes";
            defname = "scene.xml";

        }
        else
        {
            obj = Selection.gameObjects;
            repo = "preset";
            defname = "preset.xml";
        }

        var sfolder = Application.dataPath + "/PLATFORM/XML/" + repo;
        string path ="";
        if (scenename == null)
            path = EditorUtility.SaveFilePanel("filename to save", sfolder, defname, "xml");
        else
            path = sfolder + scenename;


        //Directory.CreateDirectory(path);
        foreach (object o in obj)
        {
            go = (GameObject)o;
            tbs = (blocksetup)go.GetComponent(typeof(blocksetup));

            if (tbs)
            {
                // do not forget to update paramblock value before saving 
                var filepath = path + "/" + tbs.paramblock.guid + ".xml";
                tbs.paramblock.last_pos = go.transform.position; // make sure the pos is right 
                tbs.paramblock.orig_transform = go.transform.rotation;
                scenetosave.cluster.pblist.Add(tbs.paramblock);
                //tbs.Save(filepath);
                //Debug.Log(filepath);
            }

        }

        scenetosave.Save(path);
        return true;

    }









    public bool loadscene(bool preset = false,string scenename =null)
    {
        string repo = "";
        if (!preset)
            repo = "scenes";
        else
            repo = "preset";
        var sfolder = Application.dataPath + "/PLATFORM/XML/" + repo;
        string path ="" ;
        scenecluster S = new scenecluster();
        if ( scenename == null) 
            path = EditorUtility.OpenFilePanel("load scene", sfolder, "xml");
        else
            path = sfolder + scenename;
        S = Scene.Load(path);
        foreach (ParameterBlock blk in S.pblist )
        {
            GameObject tgo = (GameObject)Resources.LoadAssetAtPath(("Assets" + selectedbasename + blk.assetname + ".fbx"), typeof(GameObject));
            blocksetup tbs = new blocksetup();
            tbs = (blocksetup)tgo.GetComponent(typeof(blocksetup));
            GameObject instance = (GameObject)Instantiate(tgo, blk.last_pos, blk.orig_transform);
            instance.name = blk.assetname + instance.GetInstanceID();
            instance.AddComponent(typeof(blocksetup));
            tbs = (blocksetup)instance.GetComponent(typeof(blocksetup));
            tbs.paramblock = blk;
        }
        return true;
    }

    void AddBlockSetupComponent(GameObject obj, blocksetup tgs)
    {
        // ****************************************************************************
        // add a custom script on gameobject 
        // this function should be called for every new block in the scene
        MeshFilter M;
        obj.AddComponent(typeof(blocksetup)); 				// refer to block setup script ( block behaviour ) 
        bs = (blocksetup)obj.GetComponent(typeof(blocksetup));		// bs is at global scope 
        M = (MeshFilter)obj.GetComponent(typeof(MeshFilter));			// get some info from mesh to custom script 
        // -------------------------------------------------------------------------------------- init the block properties  ( parameterblock ) 
        if (tgs !=null ) // c
        {
            bs.paramblock = tgs.paramblock;
            bs.paramblock.block_size = M.renderer.bounds.size; 	// change size for all 
        }
        else
        {
            bs.paramblock.block_size = M.renderer.bounds.size; 	// change size for all 
            bs.block_transform = obj.transform;	 	// transform from source	
            bs.paramblock.testfloat = UIPB.testfloat; 					// //debug value 
            bs.paramblock.editsub = editsub;							// block frozen for edition
            bs.paramblock.orig_transform = obj.transform.rotation; // origin transform help to re initialize a block in static mode 
            bs.paramblock.orig_pos = obj.transform.position;		// same for pos ( sound weird could be done in oneline have to see that 	
            bs.scenerefobj = obj;
        }

        string str = obj.name;										// change the name  
        string[] strarray = str.Split(new char[] { '-' });
        bs.paramblock.assetname = strarray[0];								// final name is block original name plus unique id 

        Guid g;
        g = Guid.NewGuid();
        bs.paramblock.guid = g.ToString();//obj.GetInstanceID().ToString();

        UIPB = bs.paramblock;

    }





    void DoBlockMove(bool instanciate, Vector3 dir)
    {
        //************************************************************************************************
        // perform block manipulation in move block section of the tool 
        // there s 2 diferent way to move a block during the runtime ( dynamicaly ) have to pay atention to 
        // rotation blocks where there s a bunch of generated sub component ( root node and looktarget ) 
        // rotation blocks use a lookat function which is more convenient to handle INSTANCIATE if the block 
        // nedd to be duplicated 

        string str;


        //if (b_followpath)
           // bs.paramblock.pathnodes[bs.paramblock.targetindex] = Selection.activeGameObject.transform.position;

        // iterate on the selection 
        for (i = 0; i < Selection.gameObjects.GetLength(0); i++)
        {
            GameObject ts = (GameObject)Selection.gameObjects.GetValue(i);
            bs = (blocksetup) ts.GetComponent("blocksetup");						// should be there 
            if (bs == null)
                continue;


            if (instanciate)
            {	// ------------------------- MOVE AND DUPLICATE

                GameObject obj = (GameObject)Instantiate(Selection.gameObjects[i], Selection.gameObjects[i].transform.position, Selection.gameObjects[i].transform.rotation);
                //blocksetup tbs = (blocksetup)Selection.gameObjects[i].GetComponent(typeof(blocksetup));
                str = Selection.gameObjects[i].name;										// change the name  
                string[] strarray = str.Split(new char[] { '-' });
                obj.name = strarray[0] + obj.GetInstanceID();								// final name is block original name plus unique id 
                //obj.transform.Equals(  Selection.gameObjects[i].transform );		// place
                if (Selection.gameObjects[i].transform.parent)
                    obj.transform.parent = Selection.gameObjects[i].transform.parent;
                //DestroyImmediate(obj.GetComponent("blocksetup"));
                blocksetup tbs = (blocksetup)obj.GetComponent("blocksetup");
                Guid g;
                g = Guid.NewGuid();
                if ( tbs )
                    tbs.paramblock.guid = g.ToString();//obj.GetInstanceID().ToString();
                //AddBlockSetupComponent(obj, null);
            }
           

            if (!UIPB.editsub)																// but test anyway 
                for (int c = 0; c < bs.paramblock.pathnodes.Count; c++)
                    bs.paramblock.pathnodes[c].pos += dir;

            // but out of the loop have to move everything but the rotating blocks 
            // todo : would be good to simplifie this with future blocks behaviours 


  

            Selection.gameObjects[i].transform.position += dir;

            Repaint();




        }

    }


    void GetDir()
    {
        // **************************************************************************************
        // make sure that the direction buttons of the move tool are related to the camera 
        // did not search a better way to manage this but i m sure there s something more 
        // simple to do directly in the moveblock function 
        // todo this function work fine but it s definitely not the correct method have to define proper transformation 
        // for all cases stick in world coordinate and cook on the fly sorry for this it s one of the first function defined in this tool 
        // but actually makeit complicated to evolve  especially cause its picky to know what would come out of 3dsmax 
        // need to ensure a consistent transform along the tool chain < huge > 

        if (SceneView.currentDrawingSceneView == null) return;
        Transform cam = SceneView.currentDrawingSceneView.camera.transform;
        Vector3 flatcamvector = new Vector3(cam.forward.x, 0.0f, cam.forward.z);
        float AF = Vector3.Angle(flatcamvector, Vector3.forward);
        float AB = Vector3.Angle(flatcamvector, Vector3.back);
        float AL = Vector3.Angle(flatcamvector, Vector3.left);
        float AR = Vector3.Angle(flatcamvector, Vector3.right);

        float [] anglearray = new float [] { AF, AB, AL, AR };
        
        Array.Sort(anglearray);
        if (AF == anglearray[0])
        {
            front = Vector3.forward;
            back = Vector3.back;
            left = Vector3.right;
            right = Vector3.left;
            b_fx = false;
        }
        if (AB == anglearray[0])
        {
            front = Vector3.back;
            back = Vector3.forward;
            left = Vector3.left;
            right = Vector3.right;
            b_fx = false;
        }
        if (AL == anglearray[0])
        {
            front = Vector3.left;
            back = Vector3.right;
            left = Vector3.forward;
            right = Vector3.back;
            b_fx = true;
        }
        if (AR == anglearray[0])
        {
            front = Vector3.right;
            back = Vector3.left;
            left = Vector3.back;
            right = Vector3.forward;
            b_fx = true;
        }

    }



    Vector3 CalculateSelectionSize(GameObject[] gameobjects) 
	{
	// **************************************************************************************
	// return the global size of given obj array used in block move section   
	// 
	 
		if ( gameobjects == null ) 										// called on GUI event may be for nothing 
			return Vector3.zero;
		


		ArrayList xar  =  new ArrayList();									// vect array for bbox
		ArrayList yar  =  new ArrayList();
		ArrayList zar  =  new ArrayList();
		
		
		for (var i = 0;i<gameobjects.GetLength(0);i++) 
		{
            GameObject TGO = (GameObject)gameobjects[i];


			MeshFilter M =(MeshFilter) TGO.GetComponent("MeshFilter");				// push all bbox in minmax array 
			if ( M == null ) continue ;									// at least if the object got a mesh render 
			
			xar.Add ( (float) M.renderer.bounds.max.x );
            xar.Add((float)M.renderer.bounds.min.x);
            yar.Add((float)M.renderer.bounds.max.y);
            yar.Add((float)M.renderer.bounds.min.y);
            zar.Add((float)M.renderer.bounds.max.z);
            zar.Add((float)M.renderer.bounds.min.z);
									
		}
		if ( xar.Count == 0 ) 											// nothing come >>exit 
            return Vector3.zero;
		
			xar.Sort();													// tidy it up 
			yar.Sort();
			zar.Sort();

            Vector3 tv = new Vector3(0.0f, 0.0f, 0.0f);				// global bbox

            tv.x = (float)xar[xar.Count - 1] - (float)xar[0];
            tv.y = (float)yar[xar.Count - 1] - (float)yar[0];
            tv.z = (float)zar[xar.Count - 1] - (float)zar[0];
			
			return tv; 
	}



    public bool ReadAssetBase(  String caller )
    {
	// **************************************************************************
	// the asset base xml file is generated by 3dsmax exporter in asset folder 
	// its basically a list of all fbx under that specific path 
	// should be replaced soon by a multi base selector 
	// this function populate the global data array ( name of asset ) 
	
	//debug.Log(EditorWindow.focusedWindow.title.ToString()+ " " + caller );

    if (data != null)
	data.Clear();  // clear the base 
	
    // next statment is a little patch should evolve to a multi base management 
    String  filepath = Application.dataPath+selectedbasename+"AssetList"+".xml";
    // use xml parser
    //debug.Log(filepath);


    XmlDocument  xmlDoc = new XmlDocument();
    if(File.Exists (filepath))
    {

        xmlDoc.Load( filepath ) ;
        if (xmlDoc == null)
            return false;

        // populate the base 
	   	XmlNodeList  Asset_list  = xmlDoc.GetElementsByTagName("AssetsList");
	   	
	   	if (  Asset_list == null)
        {
	   	//debug.Log("asset list file is corrupted"); 
	   	return false ;
        }
		XmlNodeList Item_list = Asset_list.Item(0).ChildNodes;
		
		for(i = 0; i < Item_list.Count; i++)  
		{
            XmlNodeList  l = Item_list.Item(i).ChildNodes;
		    data.Add(l[0].InnerText);
		}
		////debug.Log( data.length.ToString()  + " nb of elements " ) ;
        return true;
	}
	else 
		//debug.Log( data.Count.ToString()  + " no file " ) ;

    return false;
    }


    public void BrowseAsset (  int next , String  caller ) 								
    {
	    // ***************************************************************
	    // browse asset from base todo  should be changeed to manage multiple base 
	
	    if (  b_groupselectmode )  return ; 	
	    GameObject tgo  = (GameObject) Selection.activeObject ; 

	    if ( tgo  == null ) 
	    {
		    //debug.Log( "no object selected "   )  ;
		    return ;
	    }
	
	    ReadAssetBase("BrowseAsset()" + "from "+ caller) ;  																	// read the base definition ( generated by 3dsmax during  export ) 
	
	    blocksetup bs = (blocksetup) Selection.gameObjects[0].GetComponent( "blocksetup" );							// first element of the selection is used as active transform to pop objects 
	    if ( bs  == null ) 
	    {
		    //debug.Log( "no block setup script on source object " + Selection.activeObject.name  )  ;
		    return ; 
	    }	
	
	
	    int index ;
	    if ( Mathf.Abs( next) < 2   ) 
	    {				
	    assetbaseindex = Mathf.Abs( assetbaseindex + next )  ;								// loop index todo check index base seems to have a small bug here 
	     index =   assetbaseindex %data.Count ;
	    }
	    else 
		    index = slideindex ;
	
	    ////debug.Log(index.ToString());
	
	
	    string assetname   = (  data[index] );													// load and swap 
	
	    GameObject prefab = (GameObject) Resources.LoadAssetAtPath( ( "Assets"+selectedbasename  + assetname +".fbx") , typeof( GameObject));
	    if ( prefab == null )
	    {
		    //debug.Log( "prefab load fail "+  ( "Assets"+selectedbasename  + assetname +".fbx") );
		    return;
	    }
		    // the new instance need a block controller as well and get whatever it can grab on the 
		    // original object // in AddBlockSetupComponent funbtion ( add new fresh script ut can also take props from source 
            GameObject instance = (GameObject)Instantiate(prefab, Selection.activeTransform.position, Selection.gameObjects[0].transform.rotation);
		    instance.name = prefab.name + instance.GetInstanceID();
            AddBlockSetupComponent(instance, bs);
		    DestroyImmediate ( Selection.activeObject ) ;
		    Selection.activeGameObject = instance ;
			
    }	



    public void placefromxmlfile ()
    {
	    // **************************************************************************
	    // the asset base xml file is generated by 3dsmax exporter in asset folder 
	    // this function place assets from unity base related to the assettransfert.xml generated by max tool 
	    String filepath   = Application.dataPath+selectedbasename+"assettransfert"+".xml";
	    //debug.Log(Application.dataPath);
	    XmlDocument xmlDoc   = new XmlDocument();
        if(File.Exists (filepath))
        {
          xmlDoc.Load( filepath ) ;
	   	    if ( xmlDoc == null  ) 
	   	    {
	   		    //debug.Log( "xml load fail : " + filepath ) ;
	   		    return ;
	   	    }
	   	    XmlNodeList objlist  = xmlDoc.GetElementsByTagName("ObjectList");
		    XmlNodeList  Item_list  = objlist.Item(0).ChildNodes;

            ArrayList tempobjarray = new ArrayList();

		    for(int vi = 0; vi < Item_list.Count; vi++)  
		    {
		            XmlNodeList l  = Item_list.Item(i).ChildNodes;
		            //debug.Log(l[0].InnerText);
				    GameObject prefab = (GameObject ) Resources.LoadAssetAtPath( ( "Assets" +selectedbasename + l[0].InnerText +".fbx") , typeof(GameObject));
				    if ( prefab == null )	
				    {
					    //debug.Log(  ( "Assets/" +selectedbasename + l[0].InnerText +".fbx") );
					    return;
				    }
				    // --------------------------------------------------------- MAX >> UNITY translate roughly 
				    Vector3 pos   ; 
				    pos.z = float.Parse(l[1].InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat);  
				    pos.x = float.Parse(l[2].InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				    pos.y = float.Parse(l[3].InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				    pos.z = ( - pos.z ) ; // flip on z 
				
				    var xa = float.Parse(l[4].InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+270;
				    var ya = float.Parse(l[6].InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
				    var za = float.Parse(l[5].InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+270;
				    var rot = Quaternion.Euler (xa, ya, za);

                    GameObject instance = (GameObject) Instantiate(prefab, pos, rot);
		            instance.name = prefab.name+instance.GetInstanceID() ;

                    AddBlockSetupComponent(instance, null);
		        
		        
		            if ( b_groupselectmode ) 
		            {
		        	    tempobjarray.Add (instance) ;
		        	    ////debug.Log( tempobjarray.length.ToString());
		            }
		        
		    }
		
		    if ( b_groupselectmode ) 
		    {
				    ////debug.Log( b_groupselectmode.ToString() ) ;
                    for ( int i =0 ; i<tempobjarray.Count ; i++ )
				        Selection.objects.SetValue( tempobjarray[i],i) ;

                    GameObject tgo = (GameObject) Selection.activeGameObject;
			 	    //debug.Log(tgo.name);
			 	    for ( var c = 0 ; c< Selection.gameObjects.GetLength(0) ;c++ ) 
				    {
                        blocksetup bs = (blocksetup)  Selection.gameObjects[c].GetComponent("blocksetup");     
					    Selection.gameObjects[c].transform.parent = tgo.transform ; 
				 	    bs.parent = tgo ;
				 	    bs.paramblock.grouped = true ;
				    }
		    }
		
		
		
        }
        else 
        {	
    	    //debug.Log("assettransfert.xml not there") ;
        }
    }





    bool InitGUIValues() 
	{
		
		//  todo fill the array with all png in editor folder 
		// and texture would be avaiable on name  T.B.C 
		//**************************************************


//        string[] namearray = new string[] {"arrowup","arrowdown","arrowleft","arrowright","arrowup_d","arrowdown_d","arrowleft_d","arrowright_d","duplicate"};
																																																																																																		
		
		uparrow = (Texture2D )Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowup.png", typeof(Texture2D)) ;  
		downarrow =(Texture2D ) Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowdown.png", typeof(Texture2D) ) ;  
		leftarrow =(Texture2D ) Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowleft.png", typeof(Texture2D) ) ;
		rightarrow = (Texture2D )Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowright.png", typeof(Texture2D) ) ;  
		d_uparrow = (Texture2D ) Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowup_d.png", typeof(Texture2D) ) ;  
		d_downarrow = (Texture2D ) Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowdown_d.png", typeof(Texture2D) ) ;  
		d_leftarrow = (Texture2D ) Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowleft_d.png", typeof(Texture2D) ) ;  
		d_rightarrow = (Texture2D ) Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/arrowright_d.png", typeof(Texture2D) ) ;  
		duplicate_t = (Texture2D )  Resources.LoadAssetAtPath("Assets/PLATFORM/Editor/duplicate.png", typeof(Texture2D) ) ;  

	//	bsz  = 20;			// unit for interface pos 
		
		// default base to use 
		selectedbasename ="/PLATFORM/HIGHTECH/" ;
		return true ;

		
	}


    void Update()
    {
        // **********************************************************************************************************************************************
        // update tool interface from selected object whn tool do not have the focus
        // that s part of the parameterblock management systen left to do 

        if (Selection.activeObject == null) return;
        if (EditorWindow.focusedWindow == null) // sometime there s even no editorwindow after a focus out of unity 
            return;

//        String wt = EditorWindow.focusedWindow.title;
//        var wtype = EditorWindow.focusedWindow.GetType();

        if (EditorWindow.focusedWindow.GetType().FullName == "UnityEditor.SceneView") 						// actualy another editor window have the focus 	
        {
            if (!Selection.activeGameObject)
                return;

            blocksetup bs = (blocksetup)Selection.activeGameObject.GetComponent("blocksetup");						// 	get the blocksetup script component on the game object 
            if (bs == null || EditorWindow.focusedWindow == null) 							// 	contition to perfor update ( too many update call slow down unity while the tool window is open ) 
                return;
            if (bb_dirty == false)
                return;
            if (b_groupselectmode && Selection.activeObject)
            {
                GameObject tgo = (GameObject)bs.parent;
                if (tgo != null)
                    bs = (blocksetup)tgo.GetComponent("blocksetup");
            }
            triggerscript tgscrpt;
            if (bs.triggerobject != null)
            {
                tgscrpt = (triggerscript)bs.triggerobject.GetComponent(typeof(triggerscript));
                UIPB.b_triggeronce = tgscrpt.triggeronce;
            }
            UIPB  = bs.paramblock;
            Repaint();
        }
    }

    void OnInspectorUpdate()
    {
        //***********************************************************************************
        // update inspector sheet according to the tool value 

        if (Selection.gameObjects.Length == 0 || EditorWindow.focusedWindow == null)
            return; 																// out of Unity nothing to refresh 
        var gg = Selection.activeGameObject.transform.parent;
        if (gg != null && b_groupselectmode)
        {
            if (gg.GetComponent("blocksetup") == null)
                Selection.activeObject = gg.GetChild(0);
            else
                Selection.activeObject = gg;
        }


        if ((EditorWindow.focusedWindow.title == "blockbuster"))
        {				// Block Buster got the focus: so play that funky music white boy  
            for (var i = 0; i < Selection.gameObjects.GetLength(0); i++)
            {// lay down the boogy and play that funky music 
                bs = (blocksetup)Selection.gameObjects[i].GetComponent("blocksetup");
                if (bs != null)
                {
                   // bs.paramblock = UIPB;
                    
                    /*
                    bs.paramblock.b_followpath = UIPB.b_followpath;
                    bs.paramblock.testfloat = UIPB.testfloat;
                    bs.paramblock.move_ampl = UIPB.move_ampl;
                    bs.paramblock.editsub = editsub;
                    if (bs.triggerobject) // 
                    {
                        triggerscript ts = (triggerscript)bs.triggerobject.GetComponent("triggerscript");
                        ts.triggeronce = UIPB.b_triggeronce;
                    }
                    bs.paramblock.b_rotate_X = UIPB.b_rotate_X;
                    bs.paramblock.b_rotate_Y = UIPB.b_rotate_Y;
                    bs.paramblock.b_rotate_Z = UIPB.b_rotate_Z;
                    bs.paramblock.rotationspeed = UIPB.rotationspeed;
                    bs.paramblock.rotationtempo = UIPB.rotationtempo;
                    bs.paramblock.b_revert_rotation = UIPB.b_revert_rotation;
                    */

                }

            }
            if (slideindex - 1 != oldindexstorage)
                BrowseAsset(slideindex - 1, "CalculateSelectionSize()");												// till you die ....:::..::...::...::.::.:.:.:::

            oldindexstorage = slideindex - 1;
        }

        GameObject tg = (GameObject)Selection.activeGameObject;
        if (b_groupselectmode && tg.transform.parent)
        {
            ////debug.Log("select");
            //Selection.activeObject = Selection.activeObject.transform.parent;
        }

        ////debug.Log(bs.parent.name) ;
    }






    int selectedtab=0;


void OnGUI () 
{

	// just check that all gameobjects have a block setup component  
	// make sure all head of hierachy object have a paramblock script attached  
        
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

    if (GUILayout.Button("BLOCK MOVE", EditorStyles.toolbarButton))
        selectedtab = 0;
    if (GUILayout.Button("BLOCK PROP", EditorStyles.toolbarButton))
        selectedtab = 1;
    if (GUILayout.Button("TOOLS", EditorStyles.toolbarButton))
        selectedtab = 2;
    GUILayout.EndHorizontal();

    GetDir(); // Dir defined bu 4 global vector coocked related to the camera 

    // safely assign a game object for GUI loop

        GameObject go = Selection.activeGameObject;
        if (go != null)
            if (!go.GetComponent("blocksetup") && go.transform.parent == null && go.GetComponent("MeshFilter"))
            {
                AddBlockSetupComponent(go, null);
            }
            

        if (go != null)
         {
             bs = (blocksetup)go.GetComponent(typeof(blocksetup));
            if (bs ) 
             bs.paramblock.last_pos = go.transform.position;


             S = CalculateSelectionSize(Selection.gameObjects);
         }


	//  trick to prevent deadloop when huge amount of objscts are selected 
	if ( Selection.gameObjects.Length < 10 ) 
		bb_dirty = true;
	else 
		bb_dirty = false; 
	Vector3 V  ;			// move value 
	StaticEditorFlags editorflag ;
    //GUI.Toolbar(new Rect (25, 25, 250, 30), toolbarInt, toolbarStrings);

    switch (selectedtab) 
	{
        case 0 : // BLOCK MOVE 
        if (go == null || bs==null)
        {
            GUI.Label(new Rect(0, 20, 200, 200), "SELECT A VALID GAME OBJECT");
            return;
        }

        GUI.BeginGroup (new Rect (10, bsz * 10 , 300, 600));

            b_groupselectmode = EditorGUILayout.Toggle ("GrpMode", b_groupselectmode,GUILayout.MinWidth(280), GUILayout.MaxWidth(280) );
            step = EditorGUILayout.Toggle ("fixed predefined move  ", step );									
			//------------------------------------------------------------------ SLIDER FOR FIXED OFSET MOVE #2
			stepvalue = EditorGUILayout.Slider("fixed step ",stepvalue,0,4,GUILayout.MinWidth(280),GUILayout.MaxWidth(280));
			//------------------------------------------------------------------ NEXT ASSET BUTTON #3
			if( GUILayout.Button("NEXT ASSET",GUILayout.MinWidth(280),GUILayout.MaxWidth(280)))
				BrowseAsset ( 1 , "NEXT BUTTON"); 
			//------------------------------------------------------------------ PREV ASSET BUTTON #4
            if (GUILayout.Button("PREV ASSET", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
                BrowseAsset(-1, "PREV ASSET BUTTON");
            slideindex = (int)EditorGUILayout.Slider("quick select", slideindex, 0, data.Count, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));

            if (GUILayout.Button("snapshot object", GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
            {
                 
                 Texture2D snap = AssetPreview.GetAssetPreview(Selection.activeGameObject);
                
            
            }


        GUI.EndGroup ();
	        float ofset ;
            if (GUI.Button(new Rect(bsz * 3, bsz * 5, bsz, bsz), uparrow)) //-------------- FRONT BUTTON
			{
				if ( b_fx ) ofset = S.x ; 
				else ofset = S.z ;
				if ( step ) ofset = stepvalue ; 
				V = ( front * ofset ) ;
				DoBlockMove( false , V  ) ; 
				////debug.Log( toolbarInt.ToString());
			}

            if (GUI.Button(new Rect(bsz * 3, bsz * 7, bsz, bsz), downarrow)) //-------------- 	BACK BUTTON
			{
				if ( b_fx ) ofset = S.x ; else 	ofset = S.z ;
				if ( step ) ofset = stepvalue ; 
				V = ( back * ofset ) ;
				DoBlockMove( false , V  ) ; 
			}
                
			if (GUI.Button(new Rect(bsz*2,bsz*6,bsz,bsz),leftarrow)) //-------------- 	LEFT BUTTON
			{
				if ( b_fx ) ofset = S.z ; else 	ofset = S.x ;
				if ( step ) ofset = stepvalue ; 
				V = ( left * ofset ) ;
				DoBlockMove( false , -V  ) ; 
			}
            if (GUI.Button(new Rect(bsz * 4, bsz * 6, bsz, bsz), rightarrow)) //-------------- 	RIGHT BUTTON
			{
				if ( b_fx ) ofset = S.z ; else ofset = S.x ;
				if ( step ) ofset = stepvalue ; 
				V = ( right * ofset ) ;
				DoBlockMove( false , -V  ) ; 
			}

            if (GUI.Button(new Rect(bsz * 7, bsz * 5, bsz, bsz), uparrow)) //-------------- 	FRONT BUTTON
			{
				ofset = S.y ;
				if ( step ) ofset = stepvalue ; 
				V = ( Vector3.up * ofset ) ;
				DoBlockMove( false , V  ) ; 
			}
            if (GUI.Button(new Rect(bsz * 7, bsz * 7, bsz, bsz), downarrow)) //-------------- 	DOWN BUTTON
			{
				ofset = S.y ;
				if ( step ) ofset = stepvalue ; 
				V = ( Vector3.down * ofset ) ;
				DoBlockMove( false , V  ) ; 
			}
			// -----------------------------------------------------------------------------------------	SAME WITH DUPLICATE  
            if (GUI.Button(new Rect(bsz * 3, bsz * 4, bsz, bsz), duplicate_t)) //-------------- FRONT BUTTON
			{
				if ( b_fx ) ofset = S.x ; else ofset = S.z ;
				if ( step ) ofset = stepvalue ; 
				V = ( front * ofset ) ;
				DoBlockMove( true , V  ) ; 
			}
			if (GUI.Button(new Rect(bsz*3,bsz*8,bsz,bsz),duplicate_t)) //-------------- BACK BUTTON
			{
				if ( b_fx ) ofset = S.x ; else 	ofset = S.z ;
				if ( step ) ofset = stepvalue ; 
				V = ( back * ofset ) ;
				DoBlockMove( true , V  ) ; 
			}
			if (GUI.Button(new Rect(bsz,bsz*6,bsz,bsz),duplicate_t)) //-------------- 	LEFT BUTTON
			{
				if ( b_fx ) ofset = S.z ; else 	ofset = S.x ;
				if ( step ) ofset = stepvalue ; 
				V = ( left * ofset ) ;
				DoBlockMove( true , -V  ) ; 
			}
			if (GUI.Button(new Rect(bsz*5,bsz*6,bsz,bsz),duplicate_t)) //-------------- RIGHT BUTTON
			{
				if ( b_fx ) ofset = S.z ; else ofset = S.x ;
				if ( step ) ofset = stepvalue ; 
				V = ( right * ofset ) ;
				DoBlockMove( true , -V  ) ; 
			}
				
			if (GUI.Button(new Rect(bsz*7,bsz*4,bsz,bsz),duplicate_t)) //-------------- UP BUTTON
			{
				ofset = S.y ;
				if ( step ) ofset = stepvalue ; 
				V = ( Vector3.up * ofset ) ;
				DoBlockMove( true , V  ) ; 
			}
			if (GUI.Button(new Rect(bsz*7,bsz*8,bsz,bsz),duplicate_t)) //-------------- DOWN BUTTON
			{
				ofset = S.y ;
				if ( step ) ofset = stepvalue ; 
				V = ( Vector3.down * ofset ) ;
				DoBlockMove( true , V  ) ; 
			}
				
			if (GUI.Button(new Rect(bsz*9,bsz*6,bsz,bsz),"Y")) //---------------------- ROTATE Y BUTTON  ( ZUP ?? ) 
				
			{
				for ( i = 0;i<Selection.gameObjects.GetLength(0);i++) 
				{

                    bs = (blocksetup)Selection.gameObjects[i].GetComponent("blocksetup"); // associated script 

					Selection.gameObjects[i].transform.Rotate(Vector3.up * 90 ,  Space.Self  ); 
						
				}
							
			}
				if (GUI.Button(new Rect(bsz*11,bsz*6,bsz,bsz),"X")) //----------------- ROTATE X BUTTON
			{
				for ( i = 0;i<Selection.gameObjects.GetLength(0);i++) 
				{
                    bs = (blocksetup)Selection.gameObjects[i].GetComponent("blocksetup"); // associated script 

					Selection.gameObjects[i].transform.Rotate(Vector3.left * 90 ,  Space.Self  ); 
						
				}
							
			}
				if (GUI.Button(new Rect(bsz*13,bsz*6,bsz,bsz),"Z")) ///---------------- ROTATE Z BUTTON
					
			{
				for ( i = 0;i<Selection.gameObjects.GetLength(0);i++) 
				{
                    bs = (blocksetup)Selection.gameObjects[i].GetComponent("blocksetup"); // associated script 

					Selection.gameObjects[i].transform.Rotate(Vector3.forward * 90 ,  Space.Self  );
				}
							
			}
					
			//--------------------------------------------------------------------------------------- END OF BLOCK MOVEE TAB PANNEL  
			break;
				
				
				
		case 1 : //-------------------------------------------------------------------------- BLOCK PROP 
            if (go == null || bs ==null)
            {
                GUI.Label(new Rect(0, 20, 200, 200), "SELECT A VALID GAME OBJECT");
                return;
            }

			//---------------------------------------------------------------------		<BeginGroup>  
			//-------------------------------------------------------------- 4 Controls in the group  
				GUI.BeginGroup (new Rect (10, 80 , 300, 600));


				b_groupselectmode = EditorGUILayout.Toggle ("GrpMode", b_groupselectmode,GUILayout.MinWidth(280), GUILayout.MaxWidth(280) );				
				String  filepath  ; 

                //blocksetup tblock = (blocksetup)Selection.activeGameObject.GetComponent("blocksetup");
     

					if(GUILayout.Button("SAVE",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
					{
						if ( Selection.activeGameObject == null ) 
							break;
					 	filepath  = Application.dataPath+"/PLATFORM/XML/paramblock/"+bs.paramblock.guid+".xml";
                        bs.Save(filepath);
					}	
					
					if(GUILayout.Button("LOAD",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
					{
							if ( Selection.activeGameObject == null )
								break;
						 	filepath  = Application.dataPath+"/PLATFORM/XML/paramblock/"+bs.paramblock.guid+".xml";
							//Selection.activeObject.GetComponent(blocksetup).paramblock.Load(filepath);
							if (!System.IO.File.Exists(filepath))
								return;
							ParameterBlock p = blocksetup.Load(filepath); // deserialise pblock 
                            bs.paramblock = p;
                            UIPB = p;
					}


                    UIPB.pltf_sate = (PLTF_TYPE)EditorGUILayout.EnumPopup("block action:", UIPB.pltf_sate, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));

                    switch (UIPB.pltf_sate) // CREATE APROPRIATE UI CONTENT 
							{
								case PLTF_TYPE.MOVING : // ---------------------------------------- plateform moving state  
                                    
                                    // ========== add limitation to the target index 
                                    if (bs.paramblock.targetindex >= bs.paramblock.maxhandle)
                                        bs.paramblock.targetindex = bs.paramblock.maxhandle-1;
                                    else if (bs.paramblock.targetindex < 0)
                                        bs.paramblock.targetindex = 0;

                                    
									for ( i = 0 ; i < Selection.gameObjects.GetLength(0) ; i++ ) 
									{
										StaticEditorFlags f = new StaticEditorFlags ()   ; // clear static flags
										//editorflag = (  ) ;
										GameObjectUtility.SetStaticEditorFlags(Selection.gameObjects[i], (f));
										
                                            bs =(blocksetup) Selection.gameObjects[i].GetComponent( "blocksetup"); // associated script 
                                            if (bs == null)
                                                continue;
	                                        bs.paramblock.pltf_sate = PLTF_TYPE.MOVING  ; // same enum fix as mentioned before  
   
                                            
	    							}
									//************************* controllers for moving
                                    UIPB.move_ampl = EditorGUILayout.Slider("move speed", UIPB.move_ampl, 0, 10, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
									//---------------------------------------------------------------------------------------------- RESET POSITION 
									if(GUILayout.Button("reset Position",GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
										for ( i = 0 ; i < Selection.gameObjects.GetLength(0) ; i++ ) 
										{
											bs =(blocksetup) Selection.gameObjects[i].GetComponent( "blocksetup"); // associated script 
											if ( bs != null ) 
											{ // pull back at original place ( where the go  has been spotted for the first time 
												Selection.gameObjects[i].transform.rotation = bs.paramblock.orig_transform ;
												Selection.gameObjects[i].transform.position = bs.paramblock.orig_pos;
												bs.paramblock.editsub = true ;
                                                //UIPB.editsub = true;
												
											}
										}
								//---------------------------------------------------------------------------------------------- MOVE BUTTON  		

                                    UIPB.editsub = EditorGUILayout.Toggle("Edit Path", UIPB.editsub);
                                    UIPB.ismoving = !UIPB.editsub;  
                                    //if (UIPB.editsub)
                                    if (UIPB.editsub)
                                    GUI.BeginGroup(new Rect(5, bsz * 4, 280, 600));
                                    
                                    if (bs.paramblock.pathnodes.Count == 0)
                                        bs.paramblock.targetindex = 0;
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 2, bsz * 4, bsz, bsz), "\"")) //------------------------------- FRONT
                                    {
                                        if (b_fx) ofset = S.x; else ofset = S.z;
                                        V = (front * ofset);
                                        if (step) ofset = stepvalue;
                                        DoBlockMove(false, V);
                                        bs.paramblock.pathnodes[bs.paramblock.targetindex].pos = go.transform.position;
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 2, bsz * 6, bsz, bsz), ".")) //------------------------------- back
                                    {
                                        if (b_fx) ofset = S.x; else ofset = S.z;
                                        V = (back * ofset);
                                        if (step) ofset = stepvalue;
                                        DoBlockMove(false, V);
                                        bs.paramblock.pathnodes[bs.paramblock.targetindex].pos = go.transform.position;
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 1, bsz * 5, bsz, bsz), "<")) //------------------------------- left
                                    {
                                        if (b_fx) ofset = S.z; else ofset = S.x;
                                        V = (left * ofset);
                                        if (step) ofset = stepvalue;
                                        DoBlockMove(false, -V);
                                        bs.paramblock.pathnodes[bs.paramblock.targetindex].pos =go.transform.position;
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 3, bsz * 5, bsz, bsz), ">")) //------------------------------- right
                                    {
                                        if (b_fx) ofset = S.z; else ofset = S.x;
                                        V = (right * ofset);
                                        if (step) ofset = stepvalue;
                                        DoBlockMove(false, -V);
                                        bs.paramblock.pathnodes[bs.paramblock.targetindex].pos = go.transform.position;
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 6, bsz * 4, bsz, bsz), "\"")) //------------------------------- UP
                                    {
                                        ofset = S.y;
                                        V = (Vector3.up * ofset);
                                        if (step) ofset = stepvalue;
                                        DoBlockMove(false, V);
                                        
                                        bs.paramblock.pathnodes[bs.paramblock.targetindex].pos = go.transform.position;
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 6, bsz * 6, bsz, bsz), ".")) //------------------------------- DOWN
                                    {
                                        ofset = S.y;
                                        V = (Vector3.down * ofset);
                                        if (step) ofset = stepvalue;
                                        DoBlockMove(false, V);
                                        bs.paramblock.pathnodes[bs.paramblock.targetindex].pos = go.transform.position;
                                    }
                                    // adjust pathnodes number 
                                    if (bs.paramblock.pathnodes.Count != bs.paramblock.maxhandle)
                                    {
                                        if (bs.paramblock.pathnodes.Count > bs.paramblock.maxhandle)
                                            for (int c = bs.paramblock.pathnodes.Count; c > bs.paramblock.maxhandle; c--)
                                                bs.paramblock.pathnodes.Remove(bs.paramblock.pathnodes[c-1]);
                                        for (int c = bs.paramblock.pathnodes.Count; c < bs.paramblock.maxhandle; c++)
                                        {
                                            Pathnode pn = new Pathnode();
                                            pn.ilookatpoint = 1;
                                            pn.pos = go.transform.position;
                                            bs.paramblock.pathnodes.Add(pn);
                                        }
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 12, bsz * 4, bsz * 2, bsz), ">>")) //------------ NEXT POINT
                                    {
                                        bs.paramblock.targetindex++;
                                        if (bs.paramblock.targetindex >= bs.paramblock.pathnodes.Count-1)
                                            bs.paramblock.targetindex = bs.paramblock.pathnodes.Count -1;
                                        //debug.Log(bs.paramblock.targetindex);
                                        if (bs.paramblock.pathnodes[bs.paramblock.targetindex] != null)
                                        {
                                            int ti = bs.paramblock.targetindex;
                                            int lkp = bs.paramblock.pathnodes[ti].ilookatpoint;
                                            Vector3 targetpos = bs.paramblock.pathnodes[ti].Getlookatpoint(lkp, 1.0f);
                                            go.transform.position = bs.paramblock.pathnodes[bs.paramblock.targetindex].pos;
                                            var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                                            Qr *= Quaternion.Euler(Vector3.forward);
                                            go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, Qr, 360.0f);
                                            go.transform.position = bs.paramblock.pathnodes[bs.paramblock.targetindex].pos;
                                        }
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 9, bsz * 4, bsz * 2, bsz), "<<")) //------------ NEXT POINT
                                    {
                                        bs.paramblock.targetindex--;
                                        if (bs.paramblock.targetindex < 0)bs.paramblock.targetindex = 0;
                                        if (bs.paramblock.pathnodes[bs.paramblock.targetindex] != null)
                                        {
                                            int ti = bs.paramblock.targetindex;
                                            int lkp =bs.paramblock.pathnodes[ti].ilookatpoint;
                                            Vector3 targetpos = bs.paramblock.pathnodes[ti].Getlookatpoint(lkp,1.0f) ;
                                            var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                                            Qr *= Quaternion.Euler(Vector3.forward);
                                            go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation,Qr ,360.0f);
                                            go.transform.position = bs.paramblock.pathnodes[bs.paramblock.targetindex].pos;
                                        }
                                    }
                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 12, bsz * 6, bsz * 2, bsz), "+"))
                                    {
                                        bs.paramblock.targetindex++;
                                        bs.paramblock.maxhandle += 1;
                                    }


                                    if (UIPB.editsub)
                                    if (GUI.Button(new Rect(bsz * 9, bsz * 6, bsz * 2, bsz), "-"))
                                    {
                                        // move need 2 point or switch to static 
                                        if (bs.paramblock.maxhandle > 1)
                                        {
                                            bs.paramblock.maxhandle -= 1;
                                            if (bs.paramblock.targetindex > bs.paramblock.maxhandle)
                                                bs.paramblock.targetindex = bs.paramblock.maxhandle;
                                            bs.paramblock.pathnodes.RemoveAt(bs.paramblock.targetindex);
                                            if (bs.paramblock.pathnodes.Count > bs.paramblock.targetindex)
                                            {
                                                if ( bs.paramblock.targetindex > 0 )
                                                    bs.paramblock.targetindex--;
                                                Debug.Log("remove at " + bs.paramblock.targetindex.ToString());
                                                go.transform.position = bs.paramblock.pathnodes[bs.paramblock.targetindex].pos;
                                            }
                                            else
                                                go.transform.position = bs.paramblock.pathnodes[bs.paramblock.pathnodes.Count - 1].pos;
                                        }                                        

                                    }
                                    if (bs.paramblock.pathnodes.Count > 0)
                                    {
                                        if (bs.paramblock.targetindex < 0)
                                            bs.paramblock.targetindex = 0;
                                        if (bs.paramblock.pathnodes.Count > bs.paramblock.targetindex) // protection 
                                        {
                                            bs.paramblock.pathnodes[bs.paramblock.targetindex].lookatspeed = EditorGUILayout.Slider("lookatspeed", bs.paramblock.pathnodes[bs.paramblock.targetindex].lookatspeed, 0, 10.0f, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                            UIPB.pathnodes[UIPB.targetindex].translatespeed = EditorGUILayout.Slider("translatespeed", UIPB.pathnodes[UIPB.targetindex].translatespeed, 0, 10.0f, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                            UIPB.pathnodes[UIPB.targetindex].ilookatpoint = (int)EditorGUILayout.Slider("lookat", UIPB.pathnodes[UIPB.targetindex].ilookatpoint, 0, 8, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                            if (!UIPB.ismoving)
                                            {
                                                int ti = bs.paramblock.targetindex;
                                                int lkp = bs.paramblock.pathnodes[ti].ilookatpoint;
                                                Vector3 targetpos = bs.paramblock.pathnodes[ti].Getlookatpoint( lkp , 1.0f );
                                                var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                                                Qr *= Quaternion.Euler(Vector3.forward);
                                                go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, Qr, 360.0f);
                                            }
                                        }
                                    }
                                UIPB.b_pathloop = EditorGUILayout.Toggle("loop", UIPB.b_pathloop, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
								UIPB.b_triggered= EditorGUILayout.Toggle ("have trigger", UIPB.b_triggered, GUILayout.MinWidth(280), GUILayout.MaxWidth(280) );
                                if ( UIPB.b_triggered)
                                {
                                    UIPB.b_triggeronce = EditorGUILayout.Toggle("trigger once", UIPB.b_triggeronce, GUILayout.MaxWidth(280));
                                    UIPB.colider_type = (COLIDER_TYPE)EditorGUILayout.EnumPopup("collider to use :", UIPB.colider_type, GUILayout.MinWidth(250), GUILayout.MaxWidth(250));
 									for ( i = 0 ; i< Selection.gameObjects.GetLength(0) ; i++ ) 
 									{
 										bs =(blocksetup) Selection.gameObjects[i].GetComponent( "blocksetup"); // associated script 
 										if ( bs == null ) 
 											break;
 										//MeshFilter M =(MeshFilter) Selection.gameObjects[i].GetComponent(typeof(MeshFilter)) ;
 										if (bs.triggerobject == null )  
 										{	
	 										GameObject  colholder   = new GameObject();
	 										colholder.name = ( Selection.gameObjects[i].name + "_trigger" )  ;
	 										colholder.AddComponent( "triggerscript" )  ;
	 										colholder.transform.position = Selection.gameObjects[i].transform.position ;
	 										bs.triggerobject = colholder ;
	 										triggerscript ts =(triggerscript) bs.triggerobject.GetComponent("triggerscript");
                                            ts.parentobject = Selection.gameObjects[i] ;
	 										bs.triggerobject.transform.parent = Selection.gameObjects[i].transform ;
	 										// give a ref to the trigger go to script that have collider  
	 										ts.triggerobject = colholder ;
	 									}
	 									switch ( UIPB.colider_type ) 
	 									{
	 										case COLIDER_TYPE.BOX : 
	 											if ( bs.triggerobject.GetComponent ( "BoxCollider" ) == null ) 
	 											{
	 												 //MeshFilter MF =(MeshFilter) Selection.gameObjects[i].GetComponent("MeshFilter");
                                                     //Vector3 VV=  MF.renderer.bounds.center ; 
	 												 //debug.Log(VV.ToString());
	 												 BoxCollider MBcolider = (BoxCollider) bs.triggerobject.AddComponent("BoxCollider");
	 												 //m_collider.bounds.size = M.renderer.bounds.size ;
	 												 MBcolider.isTrigger = true ; 
	 												 if ( bs.triggerobject.GetComponent("SphereCollider"))
	 												 	DestroyImmediate (bs.triggerobject.GetComponent("SphereCollider"));
			 									 	 //bs.triggerobject.GetComponent(BoxCollider).bounds.size = vp;
	 											}	
		 										break;
		 										
	 										case COLIDER_TYPE.SPHERE : 
	 											if ( bs.triggerobject.GetComponent ( "SphereCollider" ) == null ) 
	 											{
	 												 //vp = Selection.gameObjects[i].GetComponent(MeshFilter).renderer.bounds.size ; 
	 												 bs.triggerobject.AddComponent("SphereCollider");
	 												 SphereCollider MSCOLLIDER =(SphereCollider) bs.triggerobject.GetComponent("SphereCollider");
                                                        MSCOLLIDER.isTrigger = true;
	 												 if ( bs.triggerobject.GetComponent("BoxCollider"))
	 												 	DestroyImmediate (bs.triggerobject.GetComponent("BoxCollider"));
			 									 	 
	 											}
	 										break; 
	 										case COLIDER_TYPE.CAPSULE : 
	 										break; 
	 										case COLIDER_TYPE.MESH : 
	 										break; 
	 									}
	 								}
								}
								else 
								{
									for ( i = 0 ; i< Selection.gameObjects.GetLength(0) ; i++ ) 
 									{				
                                        bs.paramblock.ismoving = true;
                                        DestroyImmediate(bs.triggerobject);
									}
								}
                                if (UIPB.editsub)
                                    GUI.EndGroup();
								break ;
									
								case PLTF_TYPE.ROTATING:
								//************************************************************************ ROTATE THE CRAP 
								// need to implement a keyframes system that could be used a lot add props to exploit in blocksetup
								for ( i = 0 ; i< Selection.gameObjects.GetLength(0) ; i++ ) 
 								{	
 									StaticEditorFlags ff = new StaticEditorFlags()   ; // clear static flags
									GameObjectUtility.SetStaticEditorFlags(Selection.gameObjects[i], (ff));
                                    bs = (blocksetup)Selection.gameObjects[i].GetComponent("blocksetup"); // associated script 
                                    if (bs != null)
                                        bs.paramblock.pltf_sate = PLTF_TYPE.ROTATING; // same enum fix as mentioned before  
								}

                                UIPB.rotationstepnumber = (int)EditorGUILayout.Slider("step", UIPB.rotationstepnumber, 2, 8, GUILayout.MaxWidth(280));

                                UIPB.rotationspeed = EditorGUILayout.Slider("speed", UIPB.rotationspeed, 0.0f, 5.0f, GUILayout.MaxWidth(280));
                                UIPB.rotationtempo = EditorGUILayout.Slider("temporisation", UIPB.rotationtempo, 0.0f, 2.0f, GUILayout.MaxWidth(280));
								// editsub button shared over panels 
                                editsub = EditorGUILayout.Toggle("editsub", editsub);
                                if (editsub)
                                {
                                    UIPB.b_rotate_X = EditorGUILayout.Toggle("rotate on X", UIPB.b_rotate_X, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                    UIPB.b_rotate_Y = EditorGUILayout.Toggle("rotate on Y", UIPB.b_rotate_Y, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                    UIPB.b_rotate_Z = EditorGUILayout.Toggle("rotate on Z", UIPB.b_rotate_Z, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                    UIPB.b_revert_rotation = EditorGUILayout.Toggle("invert", UIPB.b_revert_rotation, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                                }   
								break;
								case PLTF_TYPE.STATIC:
								for ( i = 0 ; i < Selection.gameObjects.GetLength(0) ; i++ ) 
								{
									// change flags for navmesh build 
									editorflag = ( StaticEditorFlags.BatchingStatic |  StaticEditorFlags.LightmapStatic | StaticEditorFlags.NavigationStatic ) ;
									GameObjectUtility.SetStaticEditorFlags(Selection.gameObjects[i], (editorflag));
									//GameObjectUtility.SetStaticEditorFlags(Selection.gameObjects[i], (StaticEditorFlags.BatchingStatic));
									bs =(blocksetup) Selection.gameObjects[i].GetComponent( "blocksetup"); 
									if ( bs != null ) 
									{
										bs.paramblock.pltf_sate = PLTF_TYPE.STATIC  ;
										////debug.Log( "prout" );
									} 
								}		
								if(GUILayout.Button("RESET",GUILayout.MinWidth(280), GUILayout.MaxWidth(280)))
								{
								    for ( i = 0 ; i < Selection.gameObjects.GetLength(0) ; i++ ) 
								    {
									    bs =(blocksetup) Selection.gameObjects[i].GetComponent( "blocksetup"); // associated script 
									    if ( bs != null ) 
									    {
												    Selection.gameObjects[i].transform.rotation = bs.paramblock.orig_transform ;
												    Selection.gameObjects[i].transform.position = bs.paramblock.orig_pos;
									    }
								    }
								
                                }
								break;
								case PLTF_TYPE.FALLING:
								break;
							}
						GUI.EndGroup();
				break;

        case 2: //---------------------------------------------------------------------------------------  TOOLS 

            GUI.BeginGroup (new Rect (bsz, bsz*4 , 220, 300));
            b_groupselectmode = EditorGUILayout.Toggle ("GrpMode", b_groupselectmode,GUILayout.MinWidth(280), GUILayout.MaxWidth(280) );				
            activebasename =(ACTIVEBASENAME) EditorGUILayout.EnumPopup("base:", activebasename ,GUILayout.MinWidth(330), GUILayout.MaxWidth(330));
        switch (activebasename) 
        {
            case ACTIVEBASENAME.HIGHTECH :
                selectedbasename ="/PLATFORM/HIGHTECH/" ;
                break ;
            case ACTIVEBASENAME.JUNGLE :
                selectedbasename ="/PLATFORM/JUNGLE/" ;
                break ;
            case ACTIVEBASENAME.TEMPLE :
                selectedbasename ="/PLATFORM/TEMPLE/" ;
                break ;
            case ACTIVEBASENAME.SANDBOX :
                selectedbasename ="/PLATFORM/SANDBOX/" ;
                break ;
        }
        if(GUILayout.Button("FROMMAX",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
            placefromxmlfile ();

        if (GUILayout.Button("SAVES CENE", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
            savescene();
        if (GUILayout.Button("SAVES PRESET", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
            savescene(true);
 

        if (GUILayout.Button("LOAD SCENE", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
            loadscene();
        if (GUILayout.Button("LOAD PRESET", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
            loadscene(true);


		if(GUILayout.Button("GROUP",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
		{
			for ( var c = 0 ; c< Selection.gameObjects.GetLength(0) ;c++ ) 
			{
				if ( go != Selection.gameObjects[c]) 
				{  
					Selection.gameObjects[c].transform.parent = go.transform ;
                    blocksetup tbs = (blocksetup)Selection.gameObjects[c].GetComponent(typeof(blocksetup));

                    tbs.paramblock.parentgui = bs.paramblock.guid.ToString();
                    tbs.paramblock.grouped = true;

					//DestroyImmediate( Selection.gameObjects[c].GetComponent("blocksetup"));
				}
			}
		}
	
        if(GUILayout.Button("Hide Group",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
        {
	        go.SetActive(false) ;
	        hidenobjectlist.Add(go) ;
        }
		if(GUILayout.Button("Unhide All",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
		{
		    for ( int count = 0 ; count < hidenobjectlist.Count ; count ++ )
            {
                GameObject tgo = (GameObject) hidenobjectlist[count];
		        tgo.SetActive(true) ; 
            }
            hidenobjectlist.Clear();
        }
					
	    //**************************************************************************** READ WRITE PARAMETERBLOCK FILE
	
        if(GUILayout.Button("UNGROUP",GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
		{
            for ( var c = 0 ; c< Selection.activeGameObject.transform.childCount-1 ;c++ ) 
            {
                GameObject tgo =  Selection.activeGameObject.transform.GetChild(c).gameObject;
                blocksetup tbs = (blocksetup)tgo.GetComponent(typeof(blocksetup));
                tbs.paramblock.parentgui = null ;
				tbs.paramblock.grouped = false;
            }
            Selection.activeGameObject.transform.DetachChildren();	
		}
        if (GUILayout.Button("select same", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
             ArrayList  oblist= new ArrayList() ;
            // define basename selected 
            int i;
            var sname = Selection.activeGameObject.name ;
            var tab1 = sname.Split('-');
            for (var c = 0; c < allObjects.Length -1; c++)
            {
                var itname = allObjects[c].name ;
                var tab2 = itname.Split('-');
                if (tab2[0] == tab1[0])
                    oblist.Add(allObjects[c]);
            }
            GameObject[] s = new GameObject[oblist.Count];
            oblist.CopyTo(s, 0);
            Selection.objects = s;
        }
        if (GUILayout.Button("remove col", GUILayout.MinWidth(140), GUILayout.MaxWidth(140)))
            for (var c = 0; c < Selection.gameObjects.Length - 1; c++)
                DestroyImmediate(Selection.gameObjects[c].collider);
    
         
                
        //Selection.objects. = oblist;
        GUI.EndGroup();
        break ;				
    }
}

}














