

import networkx as nx
import matplotlib.pyplot as plt

G = nx.Graph()
G.add_edges_from(
    [
        (1,2), 
        (2,3), 
        (1,3), 
        (1,4) 
    ]
)
positions = { 
    1: (20, 30), 
    2: (40, 30), 
    3: (30, 10),
    4: (0, 40)
} 

positions2 = { 
    1: (25, 35), 
    2: (45, 35), 
    3: (35, 15),
    4: (5, 45)
} 

# nx.draw_networkx(G, pos=pos)

nx.draw_networkx_nodes(G, pos=positions, node_color="black", node_size=50)
nx.draw_networkx_edges(G, pos=positions, edge_color="blue", width=0.5)
nx.draw_networkx_edges(G, pos=positions2, edge_color="red", width=0.5)
nx.draw_networkx_labels(G, pos=positions, font_size=6, font_color="white") # TODO remove, instructions dont use labels

plt.subplots_adjust(left=0.0, right=1.0, bottom=0.0, top=1.0)

plt.show()




'''
# ///  Test from copilot


import matplotlib.pyplot as plt
import networkx as nx
import math

# Create a graph object
G = nx.Graph()

# Add nodes with positions (these could be your nurse and patient locations)
positions = {0: (0, 0), 1: (1, 2), 2: (2, 4), 3: (-1, 3), 4: (-2, -3)}

G.add_nodes_from(positions.keys())

# Add edges between nodes (these could represent the paths)
edges = [(0, 1), (0, 2), (0, 3), (0, 4)]

# Calculate distances between nodes as edge weights
for i in range(len(positions)):
    for j in range(i + 1, len(positions)):
        dist = math.hypot(positions[i][0] - positions[j][0], positions[i][1] - positions[j][1])
        G.add_edge(i, j, weight=dist)

# Solve TSP using Christofides algorithm
cycle = nx.approximation.traveling_salesman_problem.christofides(G, weight="weight")
edge_list = list(nx.utils.pairwise(cycle))

# Draw the graph
plt.figure(figsize=(8, 6))
nx.draw_networkx_nodes(G, pos=positions, node_color="black", node_size=200)
nx.draw_networkx_edges(G, pos=positions, edge_color="blue", width=0.5)
nx.draw_networkx_edges(G, pos=positions, edgelist=edge_list, edge_color="red", width=3)
nx.draw_networkx_labels(G, pos=positions, font_size=10, font_color="white")

plt.title("Nurse Routes in TSP")
plt.axis("off")
plt.show()

'''
