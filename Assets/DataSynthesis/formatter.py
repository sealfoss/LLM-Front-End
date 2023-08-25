import re
def my_split(s):
    return re.filter(None, re.split(r'(\d+)', s))

file = open("places_orig.txt", "r")
lines = file.readlines()
file.close()
formatted = []
for line in lines:
    split = re.split('(\d)', line, maxsplit=1)
    formatted.append(split[0].strip()+"\n")
file = open("places.txt", "w")
file.writelines(formatted)
file.close()
"""
file = open('names.txt', 'r')
lines = file.readlines()
file.close()
formated = []
for line in lines:
    split = line.split('\t')
    formated.append(split[1].strip())
    formated.append(split[2].strip())
file = open('names_formatted.txt', 'w')
for line in formated:
    file.write(line + '\n')
file.close()
print("all done")
"""