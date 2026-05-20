using UnityEngine;

public class PosBattlerDropZone : MonoBehaviour
{
    public int positionIndex; // Índice de la posición en la que se colocará la unidad
    public bool isOccupied = false; // Indica si la zona de drop está ocupada por una unidad
    public DragAndDrop currentUnit; // Referencia a la unidad que está actualmente en esta zona de drop
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
            if(currentUnit != null && currentUnit.gameObject == collision.gameObject)
            {
                isOccupied = false; // Marca la zona de drop como desocupada al salir una unidad
                currentUnit = null; // Limpia la referencia a la unidad
            }
        }     
    }
}
