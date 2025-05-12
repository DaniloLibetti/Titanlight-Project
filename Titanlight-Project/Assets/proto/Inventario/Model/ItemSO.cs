using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

    
[CreateAssetMenu]    
public class ItemSO : ScriptableObject
    
{
    [field: SerializeField]
    public bool IsStackable { get; set; } //determina se é empilhavel ou nao
    public int ID => GetInstanceID(); //ID do item
    
    [field: SerializeField]      
    public int MaxStackSize { get; set; } = 1;//valor maximo do item empilhavel

    [field: SerializeField]
    public int Price { get; set; } = 1;//valor do venda do item
        
    [field: SerializeField]
    public string Name { get; set; }//nome do item
                                    //
    [field: SerializeField] 
    public Sprite ItemImage { get; set; }//imagem do item

}

