# ................ lab.py ................

import os

def ensuredir(dir):
    if not os.path.exists(dir):
        os.makedirs(dir)

workingdir = "m:\Time Series Data"
ensuredir(workingdir)
os.chdir(workingdir)

dowdir = "H:/dev/Reps/hg/TeaHouse/TeaHouse/Resources/Dow30/"

import shutil

import teafiles
from teafiles import *

import random

demodata = dowdir + "AA.tea"

# create temperature demo data
cities = "Lima;Rio de Janeiro;Caracas;Santiago;Maracaibo;Buenos Aires;Salvador;Valencia;Fortaleza;Belo Horizonte;Cali;Guayaquil;Curitiba;Manaus;Barquisimeto;Recife;Porto Alegre;Quito;Rosario;Montevideo;Maracay;Guarulhos;Barranquilla;Santa Cruz de la Sierra;Mendoza;Campinas;La Plata;Ciudad Guayana;Cartagena"
cities = cities.split(";")

temperaturedir = "./Weather/Temperature/"
ensuredir(temperaturedir)
humiditydir = "./Weather/Humidity/"
ensuredir(humiditydir)

values = []
f = open("H:/dev/Reps/hg/TeaHouse/TeaFile.Py.Lab/tempdata.txt")
for line in f:
    value = float(line.rstrip())
    values.append(value)

for city in cities:
    filename = temperaturedir + city + ".tea"
    t = DateTime(1995, 1, 1)
    with TeaFile.create(filename, "Time Temp", "qd", "Temperatures in " + city, {"decimals":2, "Sensor" : city + " 4B"}) as tfw:
        for value in values:
            tfw.write(t, value + random.random() * 7)
            t = t + Duration(days=1)

# clone the file for umidity as well
shutil.rmtree(humiditydir)
shutil.copytree(temperaturedir, humiditydir)

# operations

def createOperationLog(dir, servername, n = 100):
    filename = dir + servername + ".tea"
    value = 500
    with TeaFile.create(filename, "Time Max Min", "qdd", "Load at machine " + servername, {"decimals":2, "node" : "Ireland"}) as tfw:
        for t in rangen(DateTime(2000, 9, 1), Duration(seconds=1), n):
            r = random.random() - .5
            value += r * 50
            tfw.write(t, value, value * (random.random() / 5))

opdirserver = "./Operations/Servers/"
ensuredir(opdirserver)

for i in range(1, 50):
    createOperationLog(opdirserver, "xenos" + str(i), 2000)

opdirnet = "./Operations/Network Stats/"
ensuredir(opdirnet)

# createOperationLog(opdirnet, "tcp.income", 1000000000)

# copy dow
def clonedow(clonedir):
    if os.path.exists(clonedir):
        return
    shutil.copytree(dowdir, clonedir)

clonedow("./Finance/DOW 30")
clonedow("./Finance/DAX")
clonedow("./Finance/FTSE 100")
clonedow("./Finance/S&P 500")
clonedow("./Finance/NASDAQ 100")
clonedow("./Finance/CAC 40")
clonedow("./Finance/NIKKEI 225")

ensuredir("Analysis/Seasonality")
ensuredir("Analysis/Aggregation")
ensuredir("Analysis/Lab")

for i in range(2000, 2012):
    for m in ["jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"]:
        for d in range(1,30):
            os.makedirs(opdirserver + str(i) + "/" + str(m) + "/" + str(d))




import os
os.chdir("m:/time series data")
os.mkdir("lab")
os.rmdir("lab")
os.mkdir("lab")
os.chdir("lab")
os.getcwd()


import teafiles
from teafiles import *
tf = TeaFile.create("demoseries.tea", "Time Price Quantity", "qdd", "this is the demo series!", {"decimals":2, "someprop": "someval"})
i = 700
for t in rangen(DateTime(2000, 1, 1), Duration(minutes=1), 100):
    tf.write(t, i, i * 1.1)
    i += 1

tf.flush()

for t in rangen(DateTime(2001, 1, 1), Duration(minutes=1), 100):
    tf.write(t, i, i * 1.1)
    i -= 1

tf.close()

