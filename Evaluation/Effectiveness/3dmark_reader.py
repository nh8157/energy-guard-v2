import matplotlib.pyplot as plt 
import numpy as np
import os

# Path to the directory containing the plain text files
data_path = r"./results/3dmark_results"
results_path = r"./images"

# Function to read values from a file
def read_values_from_file(file_path):
    with open(file_path, 'r') as file:
        values = [float(line.split(";")[1].strip()) for line in file]
    return values

def get_data():
    # Get a list of all files in the directory
    dir_names = os.listdir(data_path)

    # Initialize a dictionary to store values from each file
    data_dict = {}

    # Read values from each file and store in the dictionary
    for dir_name in dir_names:
        dir_path = os.path.join(data_path, dir_name)
        if os.path.isdir(dir_path):
            files_names = os.listdir(dir_path)
            for file in files_names:
                if file.isnumeric():
                    values = read_values_from_file(os.path.join(dir_path, file))
                    data_dict[dir_name+"_"+file] = values
    return data_dict

if __name__ == "__main__":
    data_dict = get_data()
    for i in range(1, 5):
        plt_data = []
        plt_label = []
        title = ""
        ctr = 0
        if i == 1:
            title = "Graphic Test 1"
        elif i == 2:
            title = "Graphic Test 2"
        elif i == 3:
            title = "Physics Test"
        else:
            title = "Combined Test"

        for (key, value) in data_dict.items():
            if key.split("_")[-1] == str(i):
                ctr += 1
                plt_data.append(value)
                plt_label.append(key.split("_")[1])

        plt.boxplot(plt_data)
        plt.xticks(range(1, ctr + 1), plt_label)
        plt.xlabel("Persona Setting")
        plt.ylabel("FPS")
        filename = "FPS_" + title.replace(" ", "_")
        plt.title(title)
        if not os.path.exists("images"):
            os.mkdir("images")
        plt.savefig(os.path.join("images", filename))
        plt.clf()