import matplotlib.pyplot as plt 
import numpy as np
import os

# Path to the directory containing the plain text files
data_path = r"./results/power_consumption"
results_path = r"./images"

# Function to read values from a file
def read_values_from_file(file_path):
    with open(file_path, 'r') as file:
        values = [float(line.strip()) for line in file]
    return values

def get_data():

    # Get a list of all files in the directory
    file_names = os.listdir(data_path)

    # Initialize a dictionary to store values from each file
    data_dict = {}

    # Read values from each file and store in the dictionary
    for file_name in file_names:
        file_path = os.path.join(data_path, file_name)
        if os.path.isfile(file_path):
            values = read_values_from_file(file_path)
            data_dict[file_name] = values
        
    return data_dict

if __name__ == "__main__":
    data_dict = get_data()

    for i in range(1, 5):
        plt_data = []
        plt_label = []
        ctr = 0
        title = ""
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
        plt.xlabel("Energy Rating")
        plt.ylabel("Watts")
        filename = "Watts_" + title.replace(" ", "_")
        plt.title(title)
        if not os.path.exists("images"):
            os.mkdir("images")
        plt.title(title)
        plt.savefig(os.path.join("images", filename))
        plt.clf()