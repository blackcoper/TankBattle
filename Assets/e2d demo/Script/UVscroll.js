#pragma strict
var isVertical : boolean = false;  
var scrollSpeed : float = 20;  
  
function Update () {  
   var offset : float = Time.time * -scrollSpeed;  
   if (isVertical) {  
       renderer.material.mainTextureOffset = Vector2 (0, offset);  
   }else{  
       renderer.material.mainTextureOffset = Vector2 (offset, 0);  
   }
}
