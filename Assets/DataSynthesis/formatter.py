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