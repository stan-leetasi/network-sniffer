EXEORIG=sniffer
EXE=ipk-sniffer

SOURCE=sniffer/sniffer.csproj
OUTPUTDIR=sniffer/bin/Debug/net8.0/
TARGETDIR=$(CURDIR)

PDB=sniffer.pdb
DL=sniffer.dll
RUNTC=sniffer.runtimeconfig.json
DEPS=sniffer.deps.json
SPCAP=SharpPcap.dll
PACKDNT=PacketDotNet.dll

all: $(EXE)

$(EXE):
	sudo dotnet build $(SOURCE)
	sudo mv $(OUTPUTDIR)$(EXEORIG) $(TARGETDIR)/$(EXE)
	sudo mv $(OUTPUTDIR)$(PDB) $(TARGETDIR)
	sudo mv $(OUTPUTDIR)$(RUNTC) $(TARGETDIR)
	sudo mv $(OUTPUTDIR)$(DL) $(TARGETDIR)
	sudo mv $(OUTPUTDIR)$(DEPS) $(TARGETDIR)
	sudo mv $(OUTPUTDIR)$(SPCAP) $(TARGETDIR)
	sudo mv $(OUTPUTDIR)$(PACKDNT) $(TARGETDIR)

run:
	sudo ./ipk-sniffer

clean:
	sudo rm $(EXE) $(PDB) $(RUNTC) $(DL) $(DEPS) $(SPCAP) $(PACKDNT)
	sudo rm -r sniffer/bin
	sudo rm -r sniffer/obj

# Phony targets
.PHONY: all clean
