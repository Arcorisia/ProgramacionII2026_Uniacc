using UnityEngine;

public class PosBattlerDropZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<DragAndDrop>() != null)
        {
            Debug.Log("Colision con zona de drop");
            collision.gameObject.GetComponent<DragAndDrop>().isOverDropZone = true;
            collision.gameObject.GetComponent<DragAndDrop>().currentDropZone = this;
        }     
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<DragAndDrop>() != null)
        {
            Debug.Log("Salida de zona de drop");
            collision.gameObject.GetComponent<DragAndDrop>().isOverDropZone = false;
            collision.gameObject.GetComponent<DragAndDrop>().currentDropZone = null;
        }     
    }
}
