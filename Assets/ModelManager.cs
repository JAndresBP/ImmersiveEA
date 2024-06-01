using Meta.WitAi;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
#if (UNITY_EDITOR)
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;


public class ModelManager : MonoBehaviour
{
    public GameObject ModelRoot;
    public GameObject NodePrefab;
    public TextAsset xmlFile;
    public Material LineMaterial;
    [Header("Render Colors")]
    [Tooltip("Negocio")]
    public Color ColorNegocio = new Color(1.0f, 0.992f, 0.729f, 1.0f);
    [Tooltip("Estrategia")]
    public Color ColorEstrategia = new Color(0.953f, 0.494f, 0.475f, 1.0f);
    [Tooltip("Aplicacion")]
    public Color ColorAplicacion = new Color(0.204f, 0.792f, 0.714f, 1.0f);
    [Tooltip("Tecnologia")]
    public Color ColorTecnologia = new Color(0.733f, 0.953f, 0.475f, 1.0f);
    public bool AutoLoadModel = false;

    private float radius = 1f;
    private Dictionary<string, GameObject> elementDict = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> relationDict = new Dictionary<string, GameObject>();
    private Model model;
    private HashSet<string> selectedNodeIds = new HashSet<string>();

    // Start is called before the first frame update
    void Start()
    {

        //if (AutoLoadModel)
        //{
        //    LoadModel();
        //}else
        if(!AutoLoadModel)
        {
            elementDict.Clear();
            var nodes = ModelRoot.GetComponentsInChildren<NodeProperties>();
            foreach(var node in nodes) {
                elementDict.Add(node.identifier, node.gameObject);
            }

            relationDict.Clear();
            var relations = ModelRoot.GetComponentsInChildren<RelationProperties>();
            foreach (var relation in relations)
            {
                relationDict.Add(relation.identifier, relation.gameObject);
            }
        }
        
    }

#if (UNITY_EDITOR)
    public void LoadModel()
    {
        
        if (elementDict.Any()) {
            foreach (var element in elementDict) {
                DestroyImmediate(element.Value);
            }
            elementDict.Clear();
        }

        if (relationDict.Any()) {
            foreach (var rel in relationDict)
            {
                DestroyImmediate(rel.Value);
            }
            relationDict.Clear();
        }

        if (xmlFile != null)
        {
            var xmlString = xmlFile.text.Replace("xsi:type", "type");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Model));
            using var reader = XmlReader.Create(new System.IO.StringReader(xmlString));
            model = xmlSerializer.Deserialize(reader) as Model;

            foreach (var element in model.Elements)
            {
                var newNode = Instantiate<GameObject>(NodePrefab);
                var tmptext = newNode.GetComponentInChildren<TMP_Text>();
                tmptext.text = element.Name;
                newNode.transform.parent = ModelRoot.transform;
                newNode.transform.position = new Vector3(-3.25f, 1.2f, 1f);
                var panel = newNode.GetComponentInChildren<Image>();
                panel.color = GetNodeColor(element);
                var position = newNode.transform.position;
                position.z += UnityEngine.Random.Range(-radius, radius);
                position.x += UnityEngine.Random.Range(-radius, radius);
                position.y += GetPositionLevel(element);
                newNode.transform.position = position;

                var eventMgr = newNode.GetComponentInChildren<InteractableUnityEventWrapper>();
                if (eventMgr == null) { 
                    eventMgr = newNode.AddComponent<InteractableUnityEventWrapper>();
                }

                UnityEventTools.AddStringPersistentListener(eventMgr.WhenSelect, setSelectedNode, element.Identifier);
                UnityEventTools.AddStringPersistentListener(eventMgr.WhenUnselect, unSelectNode,element.Identifier);

                var properties = newNode.GetComponent<NodeProperties>();
                if(properties == null) { 
                    properties = newNode.AddComponent<NodeProperties>();
                }
                properties.identifier = element.Identifier;

                elementDict.Add(element.Identifier, newNode);
            }


            foreach (var relation in model.Relationships)
            {
                if (!elementDict.ContainsKey(relation.Source))
                {
                    continue;
                }

                if (!elementDict.ContainsKey(relation.Target))
                {
                    continue;
                }

                var source = elementDict[relation.Source];
                var target = elementDict[relation.Target];

                var sourceSp = source.GetComponentInChildren<SphereCollider>();
                var targetSp = target.GetComponentInChildren<SphereCollider>();


                var relgo = new GameObject(relation.Identifier);
                relgo.transform.position = new Vector3(0, 0, 0);
                relgo.transform.rotation = Quaternion.identity;

                relgo.transform.parent = ModelRoot.transform;

                var line = relgo.GetComponent<LineRenderer>();
                
                if (line == null)
                {
                    line = relgo.AddComponent<LineRenderer>();
                }

                line.startWidth = 0.005f;
                line.endWidth = 0.005f;
                line.positionCount = 2;
                line.SetMaterials(new List<Material>() { LineMaterial });

                line.SetPosition(0, sourceSp.gameObject.transform.position);
                line.SetPosition(1, targetSp.gameObject.transform.position);

                var properties = source.GetComponent<NodeProperties>();
                
                var relpro = relgo.GetComponent<RelationProperties>();
                if (relpro == null) { 
                    relpro = relgo.AddComponent<RelationProperties>();
                }
                relpro.identifier = relation.Identifier;
                relpro.target = relation.Target;
                relationDict.Add(relation.Identifier, relgo);
            }
        }
    }
#endif

    private float GetPositionLevel(Element element)
    {
        switch (element.Type)
        {
            case "Grouping":
            case "ApplicationComponent":
                return 0.6f;
            case "Capability":
                return 0.3f;
            case "BusinessActor":
            case "BusinessFunction":
            default:
                return 0.0f;

        }
    }

    private Color GetNodeColor(Element element)
    {
        switch (element.Type) {
            case "Grouping":
            case "ApplicationComponent":
                return ColorAplicacion;
            case "Capability":
                return ColorEstrategia;
            case "BusinessActor":
            case "BusinessFunction":
            default:
                return ColorNegocio;
             
        }
    }

    public void setSelectedNode(string selectedNodeId) {
        if (!selectedNodeIds.Contains(selectedNodeId))
        {
            selectedNodeIds.Add(selectedNodeId);
        }
    }

    public void unSelectNode(string nodeId) {
        if (selectedNodeIds.Contains(nodeId))
        {
            selectedNodeIds.Remove(nodeId);        
        }
    }
    // Update is called once per frame
    void Update()
    {

        if (selectedNodeIds.Any())
        {
            foreach(var nodeId in selectedNodeIds)
            {
                updateLinePosition(nodeId);
            }
        }
        
    }

    private void updateLinePosition(string nodeId)
    {
        if (!elementDict.ContainsKey(nodeId)) {
            return;
        }
        var source = elementDict[nodeId];
        var sourceSp = source.GetComponentInChildren<SphereCollider>();
        var relations = ModelRoot.GetComponentsInChildren<RelationProperties>().Where(i => i.source == nodeId);

        foreach (var relation in relations) {
            if (!relationDict.ContainsKey(relation.identifier)) {
                continue;
            }
            
            var lineGO = relationDict[relation.identifier];
            var lineComponent = lineGO.GetComponent<LineRenderer>();
            
            if (lineComponent == null) {
                continue;
            }

            lineComponent.SetPosition(0, sourceSp.gameObject.transform.position);
        }

        var targetRelations = ModelRoot.GetComponentsInChildren<RelationProperties>().Where(i => i.target == nodeId);
        foreach (var relation in targetRelations)
        {
            if (!relationDict.ContainsKey(relation.identifier))
            {
                continue;
            }

            var lineGO = relationDict[relation.identifier];
            var lineComponent = lineGO.GetComponent<LineRenderer>();

            if (lineComponent == null)
            {
                continue;
            }

            lineComponent.SetPosition(1, sourceSp.gameObject.transform.position);
        }
    }

    internal void RecalculateRelations()
    {
        if (!elementDict.Any()) {
            var nodes = ModelRoot.GetComponentsInChildren<NodeProperties>();
            foreach (var node in nodes)
            {
                elementDict.Add(node.identifier, node.gameObject);
            }
        }

        var relations = ModelRoot.GetComponentsInChildren<RelationProperties>();
        if (!relationDict.Any())
        {
            foreach (var relation in relations)
            {
                relationDict.Add(relation.identifier, relation.gameObject);
            }
        }

        var xmlString = xmlFile.text.Replace("xsi:type", "type");
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Model));
        using var reader = XmlReader.Create(new System.IO.StringReader(xmlString));
        model = xmlSerializer.Deserialize(reader) as Model;

        foreach (var relation in model.Relationships)
        {
            var go = relationDict[relation.Identifier];
            var source = elementDict[relation.Source];
            var target = elementDict[relation.Target];

            var props = go.GetComponent<RelationProperties>();
            props.target = relation.Target;
            props.source = relation.Source;

            var sourceSp = source.GetComponentInChildren<SphereCollider>();
            var targetSp = target.GetComponentInChildren<SphereCollider>();

            var line = go.GetComponent<LineRenderer>();
            line.SetPosition(0, sourceSp.gameObject.transform.position);
            line.SetPosition(1, targetSp.gameObject.transform.position);
        }
    }
}

public class Element {
    [XmlAttribute("identifier")]
    public string Identifier { get; set; }
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }
}
public class Relationship {
    [XmlAttribute("identifier")]
    public string Identifier { get; set; }
    [XmlAttribute("source")]
    public string Source { get; set; }
    [XmlAttribute("target")]
    public string Target { get; set; }
    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }
    [XmlElement(ElementName = "name")]
    public string Name { get; set; }
}
[XmlRoot("model", Namespace = "http://www.opengroup.org/xsd/archimate/3.0/")]
public class Model {
    [XmlArray("elements")]
    [XmlArrayItem("element")]
    public List<Element> Elements { get; set; }
    [XmlArray("relationships")]
    [XmlArrayItem("relationship")]
    public List<Relationship> Relationships { get; set; }
}

