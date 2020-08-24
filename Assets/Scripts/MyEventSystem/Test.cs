using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        MyEventSystem.instance.Set("v1",1);
        MyEventSystem.instance.Set("v2",5f);
        Debug.Log((int)MyEventSystem.instance.Get("v1",0,null)+" // "+(float?)MyEventSystem.instance.Get("v2",0f,null));
        MyEventSystem.instance.Register("v1", this,Fonction1);
        MyEventSystem.instance.Set("v1", 77);
        MyEventSystem.instance.RegisterToEvent("Hello", this, Fonction2);
        MyEventSystem.instance.FireEvent("Hello",new GenericDictionary().Set("arg","Comment ca va ?"));
        MyEventSystem.instance.RegisterDynamicData("test3", this, Fonction3);
        Debug.Log(MyEventSystem.instance.Get("test3",0,new GenericDictionary().Set("arg0",10).Set("arg1",11)));
        MyEventSystem.instance.Unregister(this);
        MyEventSystem.instance.FireEvent("Hello", new GenericDictionary().Set("arg", "ca va bien."));
    }

    void Fonction1(dynamic arg)
    {
        int value = default(int);
        if (arg != null)
        {
            value = arg;
        }
        Debug.Log("TEST1"+value);
    }

    void Fonction2(string name, GenericDictionary arg)
    {
        Debug.Log("TEST2 " + (string)arg.Get("arg1"));
    }

    dynamic Fonction3(GenericDictionary args)
    {
        return args.Get("arg0") + args.Get("arg1");
    }
}
