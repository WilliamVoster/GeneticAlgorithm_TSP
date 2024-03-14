
import sys
import json
import networkx as nx
import matplotlib.pyplot as plt
from matplotlib.patches import Circle


def show_plot(data):
    
    colors = [
        '#FF6347',  # Tomato
        # '#FF4500',  # OrangeRed
        '#8B4513',  # SaddleBrown
        '#FFA500',  # Orange
        '#FFD700',  # Gold
        '#FF69B4',  # HotPink
        '#FF1493',  # DeepPink
        # '#FFB6C1',  # LightPink
        '#2E8B57',  # SeaGreen
        '#FFC0CB',  # Pink
        '#DA70D6',  # Orchid
        # '#BA55D3',  # MediumOrchid
        '#CD853F',  # Peru
        '#8A2BE2',  # BlueViolet
        # '#6A5ACD',  # SlateBlue
        '#A0522D',  # Sienna
        '#483D8B',  # DarkSlateBlue
        '#4169E1',  # RoyalBlue
        # '#6495ED',  # CornflowerBlue
        '#BDB76B',  # DarkKhaki
        '#87CEEB',  # SkyBlue
        # '#00BFFF',  # DeepSkyBlue
        '#2F4F4F',  # DarkSlateGray
        '#1E90FF',  # DodgerBlue
        '#87CEFA',  # LightSkyBlue
        '#4682B4',  # SteelBlue
        '#5F9EA0',  # CadetBlue
        '#7FFF00',  # Chartreuse
        '#32CD32',  # LimeGreen
        '#228B22',  # ForestGreen
        '#008000'   # Green
    ]

    # fitness = chromosome["fitness"]
    edges = data["Item1"]
    positions_list = data["Item2"]
    positions_dict = {}

    edges = [(x[0], x[1]) for x in edges]

    G = nx.Graph()

    edge_colors = {}
    edge_index = 0
    for i, positions in enumerate(positions_list):

        if positions == None: continue

        for j, patient in enumerate(positions):
            (x, y) = edges[edge_index]

            if x > y and (x == 0 or y == 0): # for some reason G.edges() sorts only when one of the values are 0
                edge_colors[(y, x)] = colors[i]
            else:
                edge_colors[(x, y)] = colors[i]

            G.add_edge(x, y)

            positions_dict[int(patient)] = (positions[patient][0], positions[patient][1])

            edge_index += 1


    edge_color_list = []
    for edge in list(G.edges()):
        try:
            edge_color_list.append(edge_colors[edge])
        except:
            print("HERE::", edge, type(edge))
            print(G.edges())
            print("edge colors: ", edge_colors)
            print("edge_color_list: ", edge_color_list)
            exit(1)

        
    plt.figure(figsize=(10, 7.5))
    
    nx.draw(
        G,
        pos=positions_dict,
        with_labels=True, 
        node_color='black', 
        edge_color=edge_color_list, 
        width=1.0, 
        font_size=3,
        node_size=12,
        node_shape='s')
    
    # Adding a circle to the plot to represent the depot
    circle = Circle(positions_list[0]["0"], 2, color='black', fill=True)
    plt.gca().add_patch(circle)
    
    
    plt.subplots_adjust(left=0.0, right=1.0, bottom=0.0, top=1.0)
    plt.show()


if __name__ == "__main__":

    try:
        chromosome_json_file = sys.argv[1]
    except:
        print("Could not load file ")
        exit(1)

    try:
        with open(chromosome_json_file, 'r') as f:
            data = json.load(f)
    except:
        print("Could not parse json")
        exit(1)


    show_plot(data)

