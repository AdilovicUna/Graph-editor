using System.Collections.Generic;

class Graph {
    // This Dictionary maps each vertex ID to a HashSet of its neighbors.
    Dictionary<int, HashSet<int>> vertex = new Dictionary<int, HashSet<int>>();
    
    public Notify changed;  // event that fires whenever the graph changes

    // Add a new vertex to the graph and return its ID.    
    public int addVertex(int id) {
        if (id != 0 && !vertex.ContainsKey(id)){
            vertex[id] = new HashSet<int>();
        }else {
            id = 1;
        while (vertex.ContainsKey(id))
            ++id;
        vertex[id] = new HashSet<int>();
        }
        changed?.Invoke();
        return id;
    }
    
    public void connect(int i, int j) { // connect two vertices
        vertex[i].Add(j);
        changed?.Invoke();
    }   
    
    public void disconnect(int i, int j) { // disconnect two vertices
        vertex[i].Remove(j);
        changed?.Invoke();
    } 
        
    public bool areConnected(int i, int j) { 
        return vertex[i].Contains(j);
     }
    
    public IEnumerable<int> vertices() { // return enumeration of all vertices
        return vertex.Keys;
    } 
    
    public IEnumerable<int> neighbors(int id) {  // return neighbors of a vertex
        return vertex[id];
    }
    public void delete(int i) { // delete a vertex
        vertex.Remove(i);
        changed?.Invoke();
    } 

    public void deleteAll(){
        vertex.Clear();
        changed?.Invoke();
    }

    public bool contains(int id){
        return vertex.ContainsKey(id);
    }
}