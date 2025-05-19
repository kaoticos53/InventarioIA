#!/bin/bash

# Script para esperar a que la base de datos esté disponible antes de iniciar la aplicación

echo "Esperando a que la base de datos SQL Server esté disponible..."
sleep 10

echo "Iniciando contenedores..."
docker-compose up -d
