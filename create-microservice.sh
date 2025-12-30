#!/bin/bash

# Event-Driven Microservice Creator Script
# Usage: ./create-microservice.sh ServiceName EntityName [EntityPlural] [Port]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check arguments
if [ "$#" -lt 2 ]; then
    echo -e "${RED}Error: Not enough arguments${NC}"
    echo "Usage: ./create-microservice.sh ServiceName EntityName [EntityPlural] [Port]"
    echo ""
    echo "Examples:"
    echo "  ./create-microservice.sh OrderService Order Orders 5001"
    echo "  ./create-microservice.sh ProductService Product Products 5002"
    exit 1
fi

SERVICE_NAME=$1
ENTITY_NAME=$2
ENTITY_PLURAL=${3:-"${ENTITY_NAME}s"}
PORT=${4:-5001}

# Derive lowercase versions
ENTITY_LOWER=$(echo "$ENTITY_NAME" | tr '[:upper:]' '[:lower:]')
ENTITY_PLURAL_LOWER=$(echo "$ENTITY_PLURAL" | tr '[:upper:]' '[:lower:]')
DB_NAME="${ENTITY_LOWER}db"

echo -e "${GREEN}🚀 Creating new microservice: $SERVICE_NAME${NC}"
echo "   Entity: $ENTITY_NAME (plural: $ENTITY_PLURAL)"
echo "   Database: $DB_NAME"
echo "   Port: $PORT"
echo ""

# Check if source exists
if [ ! -d "Services/HakuService" ]; then
    echo -e "${RED}Error: Services/HakuService not found!${NC}"
    echo "Make sure you're running this from the template root directory."
    exit 1
fi

# Check if target already exists
if [ -d "Services/$SERVICE_NAME" ]; then
    echo -e "${RED}Error: Services/$SERVICE_NAME already exists!${NC}"
    exit 1
fi

# Copy HakuService to new service
echo -e "${YELLOW}📁 Copying template...${NC}"
cp -r Services/HakuService "Services/$SERVICE_NAME"

# Function to replace in file
replace_in_file() {
    local file=$1
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        sed -i '' -e "s/HakuService/$SERVICE_NAME/g" "$file"
        sed -i '' -e "s/Haku/$ENTITY_NAME/g" "$file"
        sed -i '' -e "s/Hakus/$ENTITY_PLURAL/g" "$file"
        sed -i '' -e "s/haku/$ENTITY_LOWER/g" "$file"
        sed -i '' -e "s/hakus/$ENTITY_PLURAL_LOWER/g" "$file"
        sed -i '' -e "s/hakudb/$DB_NAME/g" "$file"
        sed -i '' -e "s/5000/$PORT/g" "$file"
    else
        # Linux
        sed -i "s/HakuService/$SERVICE_NAME/g" "$file"
        sed -i "s/Haku/$ENTITY_NAME/g" "$file"
        sed -i "s/Hakus/$ENTITY_PLURAL/g" "$file"
        sed -i "s/haku/$ENTITY_LOWER/g" "$file"
        sed -i "s/hakus/$ENTITY_PLURAL_LOWER/g" "$file"
        sed -i "s/hakudb/$DB_NAME/g" "$file"
        sed -i "s/5000/$PORT/g" "$file"
    fi
}

# Replace in all files
echo -e "${YELLOW}🔄 Replacing placeholders...${NC}"
find "Services/$SERVICE_NAME" -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.json" -o -name "*.sln" -o -name "Dockerfile" \) | while read file; do
    replace_in_file "$file"
done

# Rename directories
echo -e "${YELLOW}📝 Renaming directories...${NC}"
find "Services/$SERVICE_NAME" -depth -type d -name "*Haku*" | while read dir; do
    new_dir=$(echo "$dir" | sed "s/Haku/$ENTITY_NAME/g")
    if [ "$dir" != "$new_dir" ]; then
        mv "$dir" "$new_dir"
    fi
done

# Rename files
echo -e "${YELLOW}📝 Renaming files...${NC}"
find "Services/$SERVICE_NAME" -type f -name "*Haku*" | while read file; do
    new_file=$(echo "$file" | sed "s/Haku/$ENTITY_NAME/g")
    if [ "$file" != "$new_file" ]; then
        mv "$file" "$new_file"
    fi
done

# Update docker-compose.yml
echo -e "${YELLOW}🐳 Updating docker-compose.yml...${NC}"
SERVICE_NAME_LOWER=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]')

cat >> docker-compose.yml << EOF

  # $SERVICE_NAME
  $SERVICE_NAME_LOWER:
    build:
      context: ./Services/$SERVICE_NAME
      dockerfile: Dockerfile
    container_name: ${SERVICE_NAME_LOWER}-api
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=$DB_NAME;Username=postgres;Password=postgres"
      ConnectionStrings__Redis: "redis:6379"
      OpenTelemetry__Endpoint: "http://otel-collector:4317"
      Localization__DefaultLanguage: "tr"
      Localization__SupportedLanguages: "tr,en,ar,de,fr"
    ports:
      - "$PORT:8080"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - haku-network
    restart: unless-stopped
EOF

echo ""
echo -e "${GREEN}✅ Microservice created successfully!${NC}"
echo ""
echo -e "${YELLOW}📋 Next steps:${NC}"
echo "1. Review the generated code in Services/$SERVICE_NAME"
echo "2. Run migrations:"
echo "   cd Services/$SERVICE_NAME/src/$SERVICE_NAME.API"
echo "   dotnet ef migrations add InitialCreate --project ../$SERVICE_NAME.Infrastructure"
echo "   dotnet ef database update"
echo ""
echo "3. Start the service:"
echo "   docker-compose up -d $SERVICE_NAME_LOWER"
echo ""
echo "4. Access API at: http://localhost:$PORT/swagger"
echo ""
echo -e "${GREEN}🎉 Happy coding!${NC}"

