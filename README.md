# MergeAtlasMeta
Unity's Editor extension for merge atlas meta(bones, sprites regions and so on). Very useful in collaborating work with other artists on the same project.<br>
<br>
You can merge metadata from two atlases in one of them. In meta stored: sprite regions, sizes, pivot points, bones, meshes and bone weights information.<br>
<br>
Also, it can fix all links in prefabs and scenes. Fix missed sprites in prefabs or change from one atlas to another.

<h1>How to use:</h1>
1) Copy atlases, prefabs, scenes in the new project<br>
2) Add all assets via explorer. <b>NOT IN UNITY EDITOR</b> (it is replace all GUIIDs). <br>
3) Then add extension <b>MergeAtlasMeta</b> in that project and run it. Add atlases in texture place.<br>
4) Open extension and tap GET METADATA. The extension will find all assets that use it and the difference between them. <br>
5) Then you need manually click each sprite that you need to migrate. <br>
6) Click UPDATE METAS. <b>Note: If in both atlases you have same sprites names extension will rewrite it.</b><br>
6) Then you need open prefabs parts and click each prefab or scene that need a switch from one atlas to another<br>
7) Done copy update assets (prefabs, scenes and atlases meta in work project).<br>
<br>
Check console if you have same errors. I'm trying to do it in a live project but unity blocked metafiles to write. In a new project, all work great.
<br>
You can move sprites with bones after migrating without problems, bones will work as usual.
